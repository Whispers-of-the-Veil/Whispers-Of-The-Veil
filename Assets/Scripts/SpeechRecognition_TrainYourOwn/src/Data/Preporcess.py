import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

import tensorflow as tf
from tensorflow import keras

from Grab_Ini import ini

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self): 
        generalConfig            = ini().grabInfo("config.ini", "General")
        self.preprocessConfig    = ini().grabInfo("config.ini", "Preprocess")

        seed            = int(generalConfig['seed'])
        vocab      = self.preprocessConfig['vocabulary']

        characters = [x for x in vocab]
        characters.append(' ')

        self.charToNum = keras.layers.StringLookup(vocabulary = characters, oov_token = "")
        self.numToChar = keras.layers.StringLookup(vocabulary = characters, oov_token = "", invert = True)

        tf.random.set_seed(seed)
        np.random.seed(seed)

    def ConvertLabel(self, _label):
        """
        This function will convert the labels back into their character form.

        Parameters:
            - _label: A list containing the label to convert

        Returns:
            The transcript of the corresponding labelc
        """
        return tf.strings.reduce_join(self.numToChar(_label)).numpy().decode("utf-8")
    
    def Data(self, _audio, _transcript):
        """
        This method is used when we are maping the audio paths and transcripts to their
        spectrograms and labels when creating a dataset in the Training script. It simply calls
        the _Audio and _Transcript methods and returns the spectrogram and label of the audio sample.

        Parameters:
            - _audio: The path of the audio file; must be a .wav file
            - _transcript: The transcript of the audio file

        Returns:
            The spectrogram and label of the audio sample
        """
        spectrogram = self.Audio(_audio)
        label = self._Transcript(_transcript)
            
        return spectrogram, label

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

            audioPath = list(data['filename'])
            transcripts = list(data['transcript'])
        except pd.errors.EmptyDataError:
            print("Error: Empty csv file or no columns to parse")
            exit(1)
        except FileNotFoundError:
            print("The csv file doesn't exist")
            exit(1)

        return audioPath, transcripts
    
    def _PlotSpec(self, _spec):
        """
        This function will use matplot to display the spectrograms to the screen

        Parameters:
            - _spec: An individual spectrogram to show
            - _index: The index number of that spectrogram
        """
        spectrogram = np.squeeze(_spec)

        plt.figure(figsize=(25, 10))
        plt.imshow(spectrogram, aspect = 'auto', origin = 'lower', cmap = 'jet')
        plt.colorbar(format='%+2.0f dB')
        plt.title(f"Spectrogram")
        plt.xlabel('Frames')
        plt.ylabel('Mel Frequency Bins')

        plt.show()
    
    def ValidateData(self, _spec, _label):
        """
        A helper function used to display the processed Spectrograms and Labels.
        Given the amount of samples the user wishes to show, it will randomly
        select those samples and print the labels to the screen, the spectrograms
        will be passed to a helper function that will display them to the user.

        Parameters:
            - _specs: A list containing the spectrograms to view
            - _labels: A list containing the labels to view
        """
        trans = self.ConvertLabel(_label)

        self._PlotSpec(_spec)

        print(f"Spec Shape: {_spec.shape}")
        print(f"Labels: {_label}")
        print(f"Transcripts: {trans}")

    # ------------------------------------------------------------------------
    #   Audio processing
    #       - Process the audio files in batches
    #       - Each batch will follow this structure
    #           - Read in the audio file
    #           - Compute the Spectrogram for each audio file
    #           - Normalize the Spectrorgam
    # ------------------------------------------------------------------------

    def Audio(self, _file):
        """
        This method transforms a given audio sample into a spectrogram using tensorflows stft method.
        It will normalize the sample before returning it.
        
        Parameters:
            - _file: The file path to a given audio sample

        Returns:
            A spectrogram
        """
        length  = int(self.preprocessConfig['frame_length'])
        step    = int(self.preprocessConfig['frame_step'])
        fft     = int(self.preprocessConfig['fft'])

        file = tf.io.read_file(_file)
        audio, _ = tf.audio.decode_wav(file)
        audio = tf.squeeze(audio, axis = -1)

        audio = tf.cast(audio, tf.float32)
        
        spectrogram = tf.signal.stft(audio, frame_length = length, frame_step = step, fft_length = fft)
        spectrogram = tf.abs(spectrogram)
        spectrogram = tf.math.pow(spectrogram, 0.5)

        return self._Normalize(spectrogram)
    
    def _Normalize(self, _spectrogram):
        """
        Normalize the features of the spectrogram

        Parameters:
            - _spectrogram: The spectrogram to normalize

        Returns:
            The normalized spectrogram
        """
        means = tf.math.reduce_mean(_spectrogram, 1, keepdims = True)
        std = tf.math.reduce_std(_spectrogram, 1, keepdims = True)

        return (_spectrogram - means) / (std + 1e-10)

    # ------------------------------------------------------------------------
    #   Transcript processing
    #
    # ------------------------------------------------------------------------

    def _Transcript(self, _transcript):
        """
        Process each transcript into its corresponding label. Where each character
        has been converted to its corresponding indice.

        Parameters:
            - _transcript: The transcript of a given audio sample

        Returns:
            The label of the transcript
        """
        trans = tf.strings.lower(_transcript)

        label = tf.strings.unicode_split(trans, input_encoding = "UTF-8")
        label = self.charToNum(label)

        return label