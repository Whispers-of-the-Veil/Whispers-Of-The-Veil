import h5py
import numpy as np
import pandas as pd
import soundfile as sf
import librosa

import concurrent.futures
from tqdm import tqdm

import tensorflow as tf
from tensorflow.keras.preprocessing.sequence import pad_sequences

import sys
import os

from Config.Grab_Ini import ini

TMP_DATA_PATH = "../Data"

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self): pass

    def SaveBatch(self, data, _index, _dirName, _outFile):
        """
        Write the provided data into a HDF5 file.

        Parameters:
            - data: The data that is to be saved
            - index: This is the particular batch number to save
            - dir_name: Name of the new directory containing the processed data
            - out_file: File prefix used to differentiate the .dat files
        """
        direcotryPath = os.path.join(TMP_DATA_PATH, _dirName)
        os.makedirs(direcotryPath, exist_ok=True)
        filePath = os.path.join(direcotryPath, f'{_outFile}_Batch{_index}.h5')

        with h5py.File(filePath, 'w') as hf:
            # Save the data
            hf.create_dataset('Data', data = data, compression = "gzip", chunks = True)


    # ------------------------------------------------------------------------
    #   Audio processing
    #       - Process the audio files in batches
    #       - Each batch will follow this structure
    #           - Read in the audio file
    #           - Compute the Mel Spectrogram for each audio file
    #           - Normalize the Mel Spectrorgam
    #           - Padd the Mel Spectrogram to a target length
    # ------------------------------------------------------------------------

    def Audio(self, _audioFiles, _targetLength, _batchSize, _dir, _outFile):
        """
        Process the audio files into Mel spectrograms, saving the results to a temparery batch file.
        This is saving the spectrograms into a temparery file to avoid holding everything in memory; the
        librispeech dataset is too large to hold all at once.
        
        Parameters:
            - _audioFiles: a list of the paths to each audio file
            - _targetLength: the target length for each spectrogram; used to pad or truncate them to a fixed length
            - _batchSize: used to determine the size of each batch
            - _dir: the name of the new directory to store the processed data
            - _outFile: is the file prefix used to determine if the batch.dat is spectrogram or labels
        """
        index = 1

        with tqdm(total = len(_audioFiles)) as pbar:
            # Divide audio files into batches that each thread will work on
            for i in range(0, len(_audioFiles), _batchSize):
                batchFiles = _audioFiles[i:i + _batchSize]
                
                with concurrent.futures.ThreadPoolExecutor() as executor:
                    futureSpectrograms = {
                        executor.submit(self.BatchAudioHelper, batchFiles, _targetLength): batchFiles
                    }
                    
                    for future in concurrent.futures.as_completed(futureSpectrograms):
                        try:
                            spectrograms = future.result()

                            self.SaveBatch(spectrograms, index, _dir, _outFile)

                            index += 1

                            pbar.update(len(spectrograms))  
                        except Exception as e:
                            print(f"Error processing batch: {e}")

    def BatchAudioHelper(self, _batchFiles, _length):
        """
        This is function defines what each thread is doing. It will read in each audio file within a givin batch
        and compute the spectrogram ensuring that it is normalized and padded/truncated to the target length

        Parameters:
            - _batchFiles: This is a batch of audio files to process
            - _length: is the target length to determine if we need to pad or truncate the spectrograms

        Returns:
            A list of Mel spectrograms for the givin batch
        """
        spectrograms = []

        for _file in _batchFiles:
            audioSample, sampleRate = sf.read(_file, dtype = 'float32')

            melSpectrogram = self.ComputeMelSpectrogram(audioSample, sampleRate, _length)
            normalizedSpectrogram = self.NormalizeZScore(melSpectrogram)
            paddedSpectrogram = self.PadSpectrograms(normalizedSpectrogram, _length)

            spectrograms.append(paddedSpectrogram)

            del melSpectrogram, normalizedSpectrogram
        
        return spectrograms
    
    def ComputeMelSpectrogram(self, _audioSample, _sampleRate, _length):
        """
        Compute the Mel spectrogram of the audio signal.
        
        Parameters:
            - _audio: An audio tensor
            - _sampleRate: The sample rate of that audio tensor
            - _length: Used to pad the the edges of the signal

        Returns:
            - A 2D NumPy array of the Mel spectrogram
        """
        # Compute the Short-Time Fourier Transform (STFT)
        spectrogram = librosa.stft(_audioSample)

        # Compute magnitude and phase, use magnitude only
        magnitude, _ = librosa.magphase(spectrogram)

         # Convert to Mel spectrogram using the Mel scale
        melScaleSpectrogram = librosa.feature.melspectrogram(S = magnitude, sr = _sampleRate, center = True, pad_mode = _length)

        # Convert amplitude to decibels
        melSpectrogram = librosa.amplitude_to_db(melScaleSpectrogram, ref = np.min)

        return melSpectrogram
    
    def NormalizeZScore(self, _melSpectrogram):
        """
        Normalizes the Mel spectrogram using z-score normalization.
        
        Parameters:
            - _melSpectrogram: The computed Mel spectrogram (2D NumPy array)
            
        Returns:
            - Normalized spectrogram with mean 0 and standard deviation 1
        """
        mean = np.mean(_melSpectrogram)
        std = np.std(_melSpectrogram)
        
        return (_melSpectrogram - mean) / std
    
    def PadSpectrograms(self, _melSpectrogram, _targetLength):
        """
        Pads or truncates the Mel spectrogram to the target length

        Parameters:
            - melSpectrogram: The computed Mel spectrogram (2D NumPy array)
            - target_length: The desired length in frames

        Returns:
            - The padded or truncated spectrogram
        """
        # Truncate the spectrogram if it is above the tragetLength
        # Otherwise Pad the spectrogram with zeros
        if _melSpectrogram.shape[1] > _targetLength:
            return _melSpectrogram[:, :_targetLength]
        elif _melSpectrogram.shape[1] < _targetLength:
            pad_width = _targetLength - _melSpectrogram.shape[1]

            return np.pad(_melSpectrogram, ((0, 0), (0, pad_width)), mode = 'constant')
        
        return _melSpectrogram

    # ------------------------------------------------------------------------
    #   Transcript processing
    #       - Process each transcript in batches. Done to match the tmp spectrogram files
    #       - Each batch will
    #           - Make the sure the transcripts are lower case. And all punctuation
    #              is removed
    #           - Create a dictionary mapping of characters to integers
    #           - Prepare the labels
    #               - Transform the transcripts into their repective indices
    #               - Pad the labels
    # ------------------------------------------------------------------------
    
    def Transcript(self, _transcripts, _batchSize, _dir, _outFile, _maxLength):
        """
        Process each transcript in batches, saving the results to temperary .data files
        
        Parameters:
            - _transcripts: A list of transcripts
            - _batchSize: used to determine the size of each batch
            - _dir: the name of the new directory to store the processed data
            - _outFile: is the file prefix used to determine if the batch.dat is spectrogram or labels
        """
        index = 1

        with tqdm(total = len(_transcripts)) as pbar:
            for i in range(0, len(_transcripts), _batchSize):
                batch = _transcripts[i:i + _batchSize]
                cleanedTranscripts = []

                try:
                    # Make sure that the transcripts are lowercase and all punctuation is removed
                    for transcript in batch:
                        trans = transcript.lower()
                        trans = ''.join(c for c in trans if c.isalnum() or c.isspace())

                        cleanedTranscripts.append(trans)

                    charToIndex = self.CreateVocabulary(cleanedTranscripts)

                    labels = self.PrepareLabels(cleanedTranscripts, charToIndex, _maxLength)

                    self.SaveBatch(labels, index, _dir, _outFile)

                    index += 1

                    pbar.update(len(batch))
                except Exception as e:
                    print(f"Error processing batch: {e}")
    
    def CreateVocabulary(self, _transcripts):
        """
        Create a dictionary mapping of characters to integer from a list of transcripts.

        Parameters:
            - _trainscripts: A list of transcripts
        
        Returns:
            returns a chracter-to-indice dictionary
        """
        uniqueChar = sorted(set(''.join(_transcripts)))
        charIndex = {char: index + 1 for index, char in enumerate(uniqueChar)}
        charIndex['<PAD>'] = 0

        return charIndex

    def PrepareLabels(self, _transcripts, _charIndex, _maxLength):
        """
        Takes in a list of transcripts, converts them into their corresponding character indices 
        using a provided character index dictionary, and pads them to ensure they all have the same length

        Parameters:
            - _transcript: A list of transcripts
            - _charIndex: A dictionary mapping characters to their respective indices
            - _maxLength: The maximum length of the sequences after padding

        Returns:
            Returns a list of processed transcripts
        """
        indexedTranscripts = self.TranscriptToIndices(_transcripts, _charIndex)
        paddedLabels = pad_sequences(indexedTranscripts, maxlen = _maxLength, padding = 'post', value = _charIndex['<PAD>'])

        return paddedLabels

    def TranscriptToIndices(self, _transcripts, _charIndex):
        """
        Converts the text transcripts into a list of integer indices using a provided a dictionary of chracter mappings

        Parameters:
            - _trascripts: A list of transcripts
            - _charIndex: A dictionary mapping characters to their respective indices
        
        Returns:
            A lists containing the integer indices for each transcript
        """
        indexedTranscripts = []

        for transcript in _transcripts:
            indexedTranscript = [_charIndex[char] for char in transcript if char in _charIndex]
            indexedTranscripts.append(indexedTranscript)

        return indexedTranscripts

def SaveH5(_spec, _label, _dirName):
    """
    Load spectrogram and label batches, and combine them into a single HDF5 file.
    Parameters:
        - dir_name: Name of the new directory to store the processed data
    """
    directoryPath = os.path.join(TMP_DATA_PATH, _dirName)

    numFiles = len(sorted(os.listdir(directoryPath)))
    numBatches = numFiles // 2

    with tqdm(total = numBatches) as pbar:
        for index in range(1, numBatches + 1):
            fileName = os.path.join(directoryPath, f"{_dirName}_Batch{index}.h5")

            # Read in each tmp h5 file and combine them into one
            with h5py.File(fileName, 'w') as hf:

                initialShape = (128, 1000, 1)
                hf.create_dataset('Spectrograms', shape = (0,) + initialShape, maxshape = (None,) + initialShape, compression = "gzip", chunks = True)

                labelShape = (350,)
                hf.create_dataset('Labels', shape = (0,) + labelShape, maxshape = (None,) + labelShape, compression = "gzip", chunks = True, dtype = 'int32')

                spectrogramBatch = os.path.join(directoryPath, f'{_spec}_Batch{index}.h5')
                labelBatch = os.path.join(directoryPath, f'{_label}_Batch{index}.h5')

                with h5py.File(spectrogramBatch, 'r') as specHF:
                    spectrograms = specHF['Data'][:]
                    spectrograms = spectrograms[..., None]

                with h5py.File(labelBatch, 'r') as labelHF:
                    labels = labelHF['Data'][:]

                # Append data to the combined file
                hf['Spectrograms'].resize(hf['Spectrograms'].shape[0] + spectrograms.shape[0], axis = 0)
                hf['Spectrograms'][-spectrograms.shape[0]:] = spectrograms

                hf['Labels'].resize(hf['Labels'].shape[0] + labels.shape[0], axis = 0)
                hf['Labels'][-labels.shape[0]:] = labels

                # Remove processed files
                os.remove(spectrogramBatch)
                os.remove(labelBatch)

                pbar.update(1)

                hf.create_dataset('InputShape', data = hf['Spectrograms'].shape)
                hf.create_dataset('OutputSize', data = hf['Labels'].shape)

def LoadCSV(_csvPath):
    """
    Loads audio files paths and transcripts from a CSV file in the form of:
	    wave_filename(path to the audio file), wave_filesize, transcript

    Parameters:
        - _csvPath: The path to the CSV file

    Returns:
        Two lists: one for audio paths and one for transcripts
    """
    data = pd.read_csv(_csvPath)

    audioPath = data['wav_filename'].tolist()
    transcripts = data['transcript'].tolist()

    return audioPath, transcripts

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("usage: python PreProcessing.py /path/to/data.csv Directory Name")
        print("Data should have the form of: filePath, fileSize, transcript")
        exit(1)

    print("Loading Config")
    generalConfig = ini().grabInfo("config.ini", "General")
    preprocessConfig = ini().grabInfo("config.ini", "Preprocess")

    seed            = int(generalConfig['seed'])
    samplesPerBatch = int(preprocessConfig['samples_per_batch'])
    targetLength    = int(preprocessConfig['spectrograms_target_length'])
    maxLength       = int(preprocessConfig['labels_max_length'])

    # File prefix for tmp .dat files
    specFileName  = "Spectrogram"
    transFileName = "Labels"

    process = Process()

    tf.random.set_seed(seed)
    np.random.seed(seed)

    print(f"\nLoading data from the csv file: {sys.argv[1]}")
    audioFiles, transcript = LoadCSV(sys.argv[1])

    print("\nConverting Audio Files into Mel Spectrograms...")
    process.Audio(audioFiles, targetLength, samplesPerBatch, sys.argv[2], specFileName)
    
    print("\nCreating Labels From the Transcripts...")
    process.Transcript(transcript, samplesPerBatch, sys.argv[2], transFileName, maxLength)

    print("\nCombining and Cleaning up temp files")
    SaveH5(specFileName, transFileName, sys.argv[2])

    print(f"\nProcessed data saved to {TMP_DATA_PATH}/{sys.argv[2]}")