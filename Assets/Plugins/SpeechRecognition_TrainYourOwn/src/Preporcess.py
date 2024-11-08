import numpy as np
import pandas as pd

import concurrent.futures
from tqdm import tqdm
import soundfile as sf
import librosa

import tensorflow as tf
from tensorflow.keras.preprocessing.sequence import pad_sequences

import psutil
import gc
import random

from scipy.ndimage import gaussian_filter

import matplotlib.pyplot as plt

from Grab_Ini import ini

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self): 
        generalConfig            = ini().grabInfo("config.ini", "General")
        self.preprocessConfig    = ini().grabInfo("config.ini", "Preprocess")

        seed            = int(generalConfig['seed'])
        self.batchSize  = int(self.preprocessConfig['samples_per_batch'])

        tf.random.set_seed(seed)
        np.random.seed(seed)

    def LoadCSV(self, _csvPath):
        """
        Loads audio files paths and transcripts from a CSV file in the form of:
            wave_filename(path to the audio file), wave_filesize, transcript

        Parameters:
            - _csvPath: The path to the CSV file

        Returns:
            Two lists: one for audio paths and one for transcripts
        """
        try:
            data = pd.read_csv(_csvPath)

            audioPath = data['wav_filename'].tolist()
            transcripts = data['transcript'].tolist()
        except pd.errors.EmptyDataError:
            print("Error: Empty csv file or no columns to parse")
            exit(1)
        except FileNotFoundError:
            print("The csv file doesn't exist")
            exit(1)

        return audioPath, transcripts
    
    def _PlotSpec(self, _spec, _index):
        spectrogram = np.squeeze(_spec)

        plt.figure(figsize=(25, 10))
        plt.imshow(spectrogram, aspect = 'auto', origin = 'lower', cmap = 'jet')
        plt.colorbar(format='%+2.0f dB')
        plt.title(f"Spectrogram of sample {_index}")
        plt.xlabel('Frames')
        plt.ylabel('Mel Frequency Bins')

        plt.show()
    
    def ValidateData(self, _specs, _labels, _numSamples):
        for _ in range(_numSamples):
            index = random.randint(0, _specs[0].shape[0])

            self._PlotSpec(_specs[index], index)

            print(f"Spec Shape: {_specs[index].shape}")
            print(f"Labels: {_labels[index]}")

    # ------------------------------------------------------------------------
    #   Audio processing
    #       - Process the audio files in batches
    #       - Each batch will follow this structure
    #           - Read in the audio file
    #           - Compute the Mel Spectrogram for each audio file
    #           - Normalize the Mel Spectrorgam
    #           - Padd the Mel Spectrogram to a target length
    # ------------------------------------------------------------------------

    def Audio(self, _audioFiles, _num, _valSet = False):
        """
        Process the audio files into Mel spectrograms, saving the results to a temporary batch file.
        This is saving the spectrograms into a temporary file to avoid holding everything in memory; the
        librispeech dataset is too large to hold all at once.
        
        Parameters:

        """
        maxAudioLength      = int(self.preprocessConfig['max_audio_length'])
        winodw              = float(self.preprocessConfig['window_length'])
        hop                 = float(self.preprocessConfig['hop_length'])
        
        # Reduce the validation set by half
        if _valSet:
            valSize = (self.batchSize - (self.batchSize // 2))
            batchFiles = _audioFiles[_num * valSize:(_num + 1) * valSize]
        else:
            batchFiles = _audioFiles[_num * self.batchSize:(_num + 1) * self.batchSize]


        spectrograms = [None] * len(batchFiles)

        try:
            with tqdm(total=len(batchFiles), desc="Spectrograms") as pbar:
                with concurrent.futures.ThreadPoolExecutor() as executor:
                    futures = {executor.submit(self._ProcessAudioFile, _file, maxAudioLength, winodw, hop): idx for idx, _file in enumerate(batchFiles)}

                    for future in concurrent.futures.as_completed(futures):
                        idx = futures[future]  # Retrieve original index

                        spectrogram = future.result()
                        spectrograms[idx] = spectrogram

                        remainingMemory = psutil.virtual_memory().available * 100 / psutil.virtual_memory().total

                        # Raise a System error if the available memory has dropped to 10% or lower
                        # This helps to prevent the system hanging or crashing
                        if (remainingMemory <= 10):
                            error = f"System has ran out of available memory: {remainingMemory}% available\n"
                            error1 = "Closing script before the system hangs or crashes\n"
                            error2 = "Try lowing the samples per batch in the ini file"

                            # Ensure that each thread joins before raising error
                            executor.shutdown(wait = True, cancel_futures = True)

                            raise SystemError(error + error1 + error2)

                        pbar.update(1)
        except SystemError as e:
            print(f"ErrorEncountered: {e}")
            del spectrograms

            exit(1)

        spectrograms = np.expand_dims(spectrograms, -1)                             # The input channels

        gc.collect()

        return spectrograms

    def _ProcessAudioFile(self, _file, _audioLength, _window, _hop):
        """
        Read an audio file, compute its Mel spectrogram, normalize it, and pad/truncate it to the target length.

        Parameters:

        Returns:
            The normalized Mel spectrogram for the given audio file
        """
        audioSample, sr = librosa.load(_file, sr = 1600)

        targetLength = int(_audioLength * sr)
        if len(audioSample) < targetLength:
            padding = targetLength - len(audioSample)
            audioSample = np.concatenate((audioSample, np.zeros(padding, dtype=np.float32)))
        elif len(audioSample) > targetLength:
            audioSample = audioSample[:targetLength]  # Truncate if necessary

        spectrogram = librosa.feature.melspectrogram(y = audioSample, sr = sr, n_mels = 160, hop_length = 160, win_length = 400)

        logSpectrogram = librosa.power_to_db(spectrogram, ref = np.max)

        return logSpectrogram
    
    # def _ComputeSpectrogram(self, _audioSample, _sampleRate, _windowLength, _hopLength):
    #     """
    #     Extract features of the audio sample using the following steps:
    #         1. Window the signal
    #         2. Apply the Discrete Fourier Transform
    #         3. Logarithm of the magnitude
    #         4. Apply discrete cosine tranform (DCT)

    #     This is done to extract both the Mel Spectrogram, and the MFCC. The MFCC will be used
    #     as the features that will be fed into the classification model. The Mel Spectrograms will be
    #     used
        
    #     Parameters:
    #         - _audio: An audio tensor
    #         - _sampleRate: The sample rate of that audio tensor
    #         - _length: Used to pad the the edges of the signal

    #     Returns:
    #         Returns a transposed Mel Spectrogram in the Logscale
    #         and the mfcc
    #     """
    #     # These are used to help align the Labels to the MFCCs of the Mel Spectrograms
    #     windowLength = int(_windowLength * _sampleRate)
    #     hopLength = int(_hopLength * _sampleRate)

    #     # Compute the Short-Time Fourier Transform (STFT)
    #     # This is a windowing step as well, it is applying a default window defined 
    #     # by librosa: Hann window
    #     # The Hann Window helps to minimize the spectral leakage when performing the Fourier
    #     # Transforms. This helps with audio signals since they are non-stationary and not perfectly periodic
    #     # This effectly taper the edges of the segment to zero.
    #     spectrogram = librosa.stft(_audioSample, n_mels = 96, n_fft = windowLength, hop_length = hopLength)

    #     logSpectrogram = librosa.amplitude_to_db(spectrogram, ref = np.max)

    #     # Return both the mel spectrograms
    #     return logSpectrogram
    
    # def _Normalize(self, _melSpectrogram):
    #     """
    #     Normalizes the Mel spectrogram using z-score normalization.
        
    #     Parameters:
    #         - _melSpectrogram: The computed Mel spectrogram (2D NumPy array)
            
    #     Returns:
    #         - Normalized spectrogram with mean 0 and standard deviation 1
    #     """
    #     # Center the spectrogram by subtracting the mean
    #     centeredSpectrogram = _melSpectrogram - np.mean(_melSpectrogram)
        
    #     # Scale the centered spectrogram to [-1, 1] by dividing by the max absolute value
    #     maxAbsValue = np.max(np.abs(centeredSpectrogram))
    #     normalizedSpectrogram = centeredSpectrogram / maxAbsValue if maxAbsValue != 0 else centeredSpectrogram
        
    #     return normalizedSpectrogram

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
    
    def Transcript(self, _transcripts, _num, _valSet = False):
        """
        Process each transcript in batches, saving the results to temperary .data files
        
        Parameters:
            - _transcripts: A list of transcripts
            - _batchSize: used to determine the size of each batch
            - _dir: the name of the new directory to store the processed data
            - _outFile: is the file prefix used to determine if the batch.dat is spectrogram or labels
        """
        index = 1

        maxTransLength      = int(self.preprocessConfig['max_transcript_length'])

        # Reduce the validation set by half
        if _valSet:
            valSize = (self.batchSize - (self.batchSize // 2))
            batch = _transcripts[_num * valSize:(_num + 1) * valSize]
        else:
            batch = _transcripts[_num * self.batchSize:(_num + 1) * self.batchSize]

        cleanedTranscripts  = []

        with tqdm(total = len(batch), desc="Labels") as pbar:
            # Make sure that the transcripts are lowercase and all punctuation is removed
            for transcript in batch:
                trans = transcript.lower()
                trans = ''.join(c for c in trans if c.isalnum() or c.isspace())

                cleanedTranscripts.append(trans)

            charToIndex = self._CreateVocabulary(cleanedTranscripts)

        
            for transcript in batch: 
                labels = self._PrepareLabels(cleanedTranscripts, charToIndex, maxTransLength)

                pbar.update(1)
        
        return labels
    
    def _CreateVocabulary(self, _transcripts):
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

    def _PrepareLabels(self, _transcripts, _charIndex, _numFrames):
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
        indexedTranscripts = self._TranscriptToIndices(_transcripts, _charIndex)
        paddedLabels = pad_sequences(indexedTranscripts, maxlen = _numFrames, padding = 'post', value = _charIndex['<PAD>'])

        return paddedLabels

    def _TranscriptToIndices(self, _transcripts, _charIndex):
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