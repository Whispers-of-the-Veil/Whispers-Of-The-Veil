# Used to handle the data in memory or to save to disk
import h5py
import numpy as np
import pandas as pd

# Used to extract features (MFCC) from the audio samples
import soundfile as sf
from scipy.ndimage import gaussian_filter
from sklearn.preprocessing import StandardScaler
from scipy.fftpack import dct
import librosa

# Used to help reduce the amount of time the algorithm takes to extract features
# And to display its progress to the user
import concurrent.futures
from tqdm import tqdm

# Used to help align the labels to the frames of the MFCC and the Mel Spectrograms
import tensorflow as tf
from tensorflow.keras.preprocessing.sequence import pad_sequences

# Used to get system paths and to explicitly call the garbage collector
import sys
import os
import gc

# Custom class, used to import the configurations of the ini file
from Config.Grab_Ini import ini

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self, _dirPath):
        self.directoryPath = _dirPath

    def SaveBatch(self, data, _index, _dirName, _outFile):
        """
        Write the provided data into a HDF5 file.

        Parameters:
            - data: The data that is to be saved
            - index: This is the particular batch number to save
            - dir_name: Name of the new directory containing the processed data
            - out_file: File prefix used to differentiate the .dat filess
        """
        direcotryPath = os.path.join(self.directoryPath, _dirName)
        os.makedirs(direcotryPath, exist_ok=True)
        filePath = os.path.join(direcotryPath, f'{_outFile}_Batch{_index}.h5')

        with h5py.File(filePath, 'w') as hf:
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

    def Audio(self, _audioFiles, _maxAudioLength, _batchSize, _dir, _numCoefficients, _windowLength, _hopLength):
        """
        Process the audio files into Mel spectrograms, saving the results to a temporary batch file.
        This is saving the spectrograms into a temporary file to avoid holding everything in memory; the
        librispeech dataset is too large to hold all at once.
        
        Parameters:
            - _audioFiles: a list of the paths to each audio file
            - _targetLength: the target length for each spectrogram; used to pad or truncate them to a fixed length
            - _batchSize: used to determine the size of each batch
            - _dir: the name of the new directory to store the processed data
            - _outFile: is the file prefix used to determine if the batch.dat is spectrogram or labels
        """
        totalBatches = (len(_audioFiles) + _batchSize - 1) // _batchSize

        with tqdm(total=totalBatches) as pbar:
            # Divide audio files into batches
            for i in range(0, len(_audioFiles), _batchSize):
                batchFiles = _audioFiles[i:i + _batchSize]
                batchIndex = i // _batchSize + 1

                # Process the batch concurrently
                spectrograms, specLengths, mfccs, mfccShape = self.BatchAudioHelper(batchFiles, _maxAudioLength, _numCoefficients, _windowLength, _hopLength)
                
                # Save the batch to the disk to help reduce the RAM usage
                self.SaveBatch(spectrograms, batchIndex, _dir, "Spectrograms")
                self.SaveBatch(mfccs, batchIndex, _dir, "MFCC")

                # Explicitly clean unneeded data and call the garbage collector to help reduce RAM usage
                del spectrograms, mfccs
                gc.collect()

                pbar.update(1)

        return specLengths, mfccShape

    def BatchAudioHelper(self, _batchFiles, _audioLength, _numCoefficients, _windowLength, _hopLength):
        """
        Process a batch of audio files into Mel spectrograms concurrently.

        Parameters:
            - _batchFiles: This is a batch of audio files to process
            - _length: is the target length to determine if we need to pad or truncate the spectrograms

        Returns:
            A list of Mel spectrograms for the given batch
        """
        spectrograms = [None] * len(_batchFiles)  # Pre-allocate list
        mfccs = [None] * len(_batchFiles)

        with tqdm(total=len(_batchFiles), desc="Processing Batch") as pbar:
            with concurrent.futures.ThreadPoolExecutor() as executor:
                futures = {executor.submit(self.ProcessAudioFile, _file, _audioLength, _numCoefficients, _windowLength, _hopLength): idx for idx, _file in enumerate(_batchFiles)}

                for future in concurrent.futures.as_completed(futures):
                    idx = futures[future]  # Retrieve original index
                    try:
                        normalizedSpectrogram, specLength, mfcc, mfccShape = future.result()
                        spectrograms[idx] = normalizedSpectrogram  # Place result in the correct index
                        mfccs[idx] = mfcc

                    except Exception as e:
                        print(f"Error processing audio file {futures[future]}: {e}")
                    finally:
                        pbar.update(1)  # Update progress bar for each processed file

        return spectrograms, specLength, mfccs, mfccShape

    def ProcessAudioFile(self, _file, _audioLength, _numCoefficients, _windowLength, _hopLength):
        """
        Read an audio file, compute its Mel spectrogram, normalize it, and pad/truncate it to the target length.

        Parameters:
            - _file: The path to the audio file
            - _length: The target length to determine if we need to pad or truncate the spectrogram

        Returns:
            The normalized Mel spectrogram for the given audio file
        """
        audioSample, sampleRate = sf.read(_file, dtype='float32')

        targetLength = int(_audioLength * sampleRate)
        if len(audioSample) < targetLength:
            padding = targetLength - len(audioSample)
            audioSample = np.concatenate((audioSample, np.zeros(padding, dtype=np.float32)))
        elif len(audioSample) > targetLength:
            audioSample = audioSample[:targetLength]  # Truncate if necessary

        melSpectrogram, mfcc, mfccShape = self.ComputeMelSpectrogram(audioSample, sampleRate, _numCoefficients, _windowLength, _hopLength)
        # smoothed = gaussian_filter(melSpectrogram, sigma=1)
        normalizedSpectrogram = self.NormalizeZScore(melSpectrogram)

        del melSpectrogram, audioSample

        return normalizedSpectrogram, normalizedSpectrogram.shape[0], mfcc, mfccShape
    
    def ComputeMelSpectrogram(self, _audioSample, _sampleRate, _numCoefficients, _windowLength, _hopLength):
        """
        Extract features of the audio sample using the following steps:
            1. Window the signal
            2. Apply the Discrete Fourier Transform
            3. Logarithm of the magnitude
            4. Apply discrete cosine tranform (DCT)

        This is done to extract both the Mel Spectrogram, and the MFCC. The MFCC will be used
        as the features that will be fed into the classification model. The Mel Spectrograms will be
        used
        
        Parameters:
            - _audio: An audio tensor
            - _sampleRate: The sample rate of that audio tensor
            - _length: Used to pad the the edges of the signal

        Returns:
            Returns a transposed Mel Spectrogram in the Logscale
            and the mfcc
        """
        # These are used to help align the Labels to the MFCCs of the Mel Spectrograms
        windowLength = int(_windowLength * _sampleRate)
        hopLength = int(_hopLength * _sampleRate)

        scaler = StandardScaler()

        # Compute the Short-Time Fourier Transform (STFT)
        # This is a windowing step as well, it is applying a default window defined 
        # by librosa: Hann window
        # The Hann Window helps to minimize the spectral leakage when performing the Fourier
        # Transforms. This helps with audio signals since they are non-stationary and not perfectly periodic
        # This effectly taper the edges of the segment to zero.
        spectrogram = librosa.stft(_audioSample, n_fft = windowLength, hop_length = hopLength)

        # This step computes the magnitude spectrum
        magnitude, _ = librosa.magphase(spectrogram)

        # Convert to Mel spectrogram using the Mel scale
        melScaleSpectrogram = librosa.feature.melspectrogram(S = magnitude, sr = _sampleRate, n_fft = windowLength, hop_length = hopLength)

        # Convert he mel spectrogram to the log scale
        # MFCC that we are trying to compute will need to be based on the logarithmic
        # perception of sound; it wont work correctly otherwise.
        logMelSpectrogram = librosa.amplitude_to_db(melScaleSpectrogram, ref = np.max)

        # Apply the DCT on each frame
        mfcc = dct(logMelSpectrogram, type = 2, axis = 1, norm = 'ortho')

        # Retain only the First N Coeffcients of the DCT
        mfcc = mfcc[:, :_numCoefficients]

        normalizedMfcc = scaler.fit_transform(mfcc)

        # Transpose the Mel Spectrogram 
        transposedSpec = logMelSpectrogram.T

        # Return both the mel spectrograms and the MFCC
        return transposedSpec, normalizedMfcc, normalizedMfcc.shape
    
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
    
    def Transcript(self, _transcripts, _numFrames, _batchSize, _dir):
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
                    outputSize = len(charToIndex)

                    labels = self.PrepareLabels(cleanedTranscripts, charToIndex, _numFrames)

                    # Save this batch to the disk to help reduce the RAM usage
                    self.SaveBatch(labels, index, _dir, "Labels")

                    index += 1

                    pbar.update(len(batch))
                except Exception as e:
                    print(f"Error processing batch: {e}")
        
        return outputSize
    
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

    def PrepareLabels(self, _transcripts, _charIndex, _numFrames):
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
        paddedLabels = pad_sequences(indexedTranscripts, maxlen = _numFrames, padding = 'post', value = _charIndex['<PAD>'])

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

def SaveH5(_dirPath, _dirName, _frequencyLength, _mfccShape, _maxLength, _size):
    """
    Load spectrogram and label batches, and combine them into a single HDF5 file.

    Parameters:
        - dir_name: Name of the new directory to store the processed data
    """
    directoryPath = os.path.join(_dirPath, _dirName)
    numFiles = len(sorted(os.listdir(directoryPath)))
    numBatches = numFiles // 3

    with tqdm(total=numBatches) as pbar:
        for index in range(1, numBatches + 1):
            fileName = os.path.join(directoryPath, f"{_dirName}_Batch{index}.h5")
            # Read in each tmp h5 file and combine them into one
            with h5py.File(fileName, 'w') as hf:
                initialShape = (_frequencyLength, 128, 1)
                hf.create_dataset('Spectrograms', shape=(0,) + initialShape, maxshape = (None,) + initialShape, compression = "gzip", chunks = True)

                hf.create_dataset('MFCC', shape = (0,) + _mfccShape, maxshape = (None,) + _mfccShape, compression = "gzip", chunks = True)

                labelShape = (_maxLength,)
                hf.create_dataset('Labels', shape=(0,) + labelShape, maxshape=(None,) + labelShape, compression = "gzip", chunks = True, dtype = 'int32')
                
                spectrogramBatch = os.path.join(directoryPath, f'Spectrograms_Batch{index}.h5')
                mfccBatch = os.path.join(directoryPath, f'MFCC_Batch{index}.h5')
                labelBatch = os.path.join(directoryPath, f'Labels_Batch{index}.h5')
                
                with h5py.File(spectrogramBatch, 'r') as specHF:
                    spectrograms = specHF['Data'][:]
                    spectrograms = spectrograms[..., None]
                with h5py.File(mfccBatch, 'r') as mfccHF:
                    mfccs = mfccHF['Data'][:]
                with h5py.File(labelBatch, 'r') as labelHF:
                    labels = labelHF['Data'][:]
                
                # Append data to the combined file
                hf['Spectrograms'].resize(hf['Spectrograms'].shape[0] + spectrograms.shape[0], axis = 0)
                hf['Spectrograms'][-spectrograms.shape[0]:] = spectrograms

                hf['MFCC'].resize(hf['MFCC'].shape[0] + mfccs.shape[0], axis = 0)
                hf['MFCC'][-mfccs.shape[0]:] = mfccs

                hf['Labels'].resize(hf['Labels'].shape[0] + labels.shape[0], axis = 0)
                hf['Labels'][-labels.shape[0]:] = labels
                
                # Remove processed files
                os.remove(spectrogramBatch)
                os.remove(mfccBatch)
                os.remove(labelBatch)
                pbar.update(1)
                
                hf.create_dataset('SpecShape', data = hf['Spectrograms'].shape)
                hf.create_dataset('InputShape', data = hf['MFCC'].shape)
                hf.create_dataset('OutputSize', data = _size)

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
    if len(sys.argv) != 4:
        print("usage: python PreProcessing.py /path/to/data.csv datasetname /path/to/save/dataset")
        print("Data should have the form of: filePath, fileSize, transcript")
        exit(1)

    process = Process(sys.argv[3])

    print("Loading Config")
    generalConfig       = ini().grabInfo("config.ini", "General")
    preprocessConfig    = ini().grabInfo("config.ini", "Preprocess")

    seed                = int(generalConfig['seed'])
    samplesPerBatch     = int(preprocessConfig['samples_per_batch'])
    maxAudioLength      = int(preprocessConfig['max_audio_length'])
    numCoefficients     = int(preprocessConfig['num_coefficients_for_mfcc'])
    windowLength        = float(preprocessConfig['window_length'])
    hopLength           = float(preprocessConfig['hop_length'])

    tf.random.set_seed(seed)
    np.random.seed(seed)

    print(f"\nLoading data from the csv file: {sys.argv[1]}")
    audioFiles, transcript = LoadCSV(sys.argv[1])

    print("\nConverting Audio Files into Mel Spectrograms, and extracting the MFCC...")
    frequencyLength, mfccShape = process.Audio(audioFiles, maxAudioLength, samplesPerBatch, sys.argv[2], numCoefficients, windowLength, hopLength)
    
    print("\nCreating Labels From the Transcripts...")
    # pass the frames of the mfcc to align the labels with them
    size = process.Transcript(transcript, mfccShape[0], samplesPerBatch, sys.argv[2])

    print("\nCombining and Cleaning up temp files")
    SaveH5(sys.argv[3], sys.argv[2], frequencyLength, mfccShape, mfccShape[0], size)

    print(f"\nProcessed data saved to {sys.argv[3]}/{sys.argv[2]}")