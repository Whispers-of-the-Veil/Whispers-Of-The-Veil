import numpy as np
import pandas as pd
import soundfile as sf
import librosa

import concurrent.futures
import threading
from tqdm import tqdm

import tensorflow as tf
from tensorflow.keras.preprocessing.sequence import pad_sequences

import sys
import os

TMP_DATA_PATH = "Data"

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self):
        self.lock = threading.Lock()

    def SaveBatch(self, _data, _index, _dir, _outFile):
        """
        Write the provided Data into a temporary npz file

        Parameters:
            - _data: The data that is to be saved
            - _index: This is the particular batch number to save
            - _dir: is the name of the new directory containing the processed data
            - _outFile: is the filePrefix used to differentiate the .dat files
        """
        directoryPath = os.path.join(TMP_DATA_PATH, _dir)
        os.makedirs(directoryPath, exist_ok = True)
        filePath = os.path.join(directoryPath, f'{_outFile}_Batch{_index}.npz')

        _data = np.array(_data)

        with self.lock:
            np.savez_compressed(
                filePath,
                Data = _data
            )

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
    
    def Transcript(self, _transcripts, _batchSize, _dir, _outFile):
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

                    maxLength = max(len(t) for t in cleanedTranscripts)
                    labels = self.PrepareLabels(cleanedTranscripts, charToIndex, maxLength)

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

def SaveNPZ(_spec, _label, _dir):
    """
    Load spectrogram and label batches, and combine them into a single .npz file

    Parameters:
        - _dir: the name of the new directory to store the processed data
    """
    directoryPath = os.path.join(TMP_DATA_PATH, _dir)
    files = sorted([f for f in os.listdir(directoryPath) if f.endswith('.npz')])
    
    # The number of batch files should be half the number of files in the _dir directory
    # since both the spectrogram and labels should have the same number of batches
    numBatches = len(files) // 2

    # Iterate over each batch for the spectrograms and labels 
    # and combine them into a single .npz fil
    with tqdm(total = numBatches) as pbar:
        for index in range(1, numBatches + 1):
            spectrogramBatch = os.path.join(directoryPath, f'{_spec}_Batch{index}.npz')
            labelBatch = os.path.join(directoryPath, f'{_label}_Batch{index}.npz')

            specData = np.load(spectrogramBatch)
            spectrograms = np.expand_dims(specData['Data'], axis = -1)

            labelData = np.load(labelBatch)
            labels = labelData['Data']

            inputShape = spectrograms.shape[1:]
            size = len(set(''.join(str(label) for label in labels)))

            np.savez_compressed(
                f"{directoryPath}/{_dir}_Batch{index}.npz",
                Spectrograms = spectrograms,
                Labels       = labels,
                InputShape   = inputShape,
                OutputSize   = size
            )

            os.remove(spectrogramBatch)
            os.remove(labelBatch)

            pbar.update(1)

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

    # Config
    targetLength    = 1000
    seed            = 42
    batchSize       = 2000

    # File prefix for tmp .dat files
    specFileName  = "Spectrogram"
    transFileName = "Labels"

    process = Process()

    tf.random.set_seed(seed)
    np.random.seed(seed)

    print(f"Loading data from the csv file: {sys.argv[1]}")
    audioFiles, transcript = LoadCSV(sys.argv[1])

    print("\nProcessing Audio files")
    process.Audio(audioFiles, targetLength, batchSize, sys.argv[2], specFileName)
    
    print("\nProcessing the Transcripts")
    process.Transcript(transcript, batchSize, sys.argv[2], transFileName)

    print("\nCombining and Cleaning up temp files")
    SaveNPZ(specFileName, transFileName, sys.argv[2])

    print(f"\nProcessed data saved to {TMP_DATA_PATH}/{sys.argv[2]}")