# Lucas Davis

import os
import sys
import numpy as np
import pandas as pd

import tensorflow as tf
from tensorflow import keras

from Grab_Ini import ini

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self):
        if getattr(sys, 'frozen', False):
            path = os.path.join(os.path.dirname(sys.executable), "config.ini")
        else:
            path = "config.ini"
        
        generalConfig          = ini().grabInfo(path, "General")
        self.processConfig     = ini().grabInfo(path, "Process")
        self.spectrogramConfig = ini().grabInfo(path, "Process.Spectrogram")
        labelConfig            = ini().grabInfo(path, "Process.Label")

        self.seed              = int(generalConfig['seed'])
        vocab                  = labelConfig['vocabulary']

        characters = [x for x in vocab]
        characters.append(' ')

        self.charToNum = keras.layers.StringLookup(vocabulary = characters, oov_token = "")
        self.numToChar = keras.layers.StringLookup(vocabulary = characters, oov_token = "", invert = True)

        tf.random.set_seed(self.seed)
        np.random.seed(self.seed)

    def ConvertLabel(self, _label):
        """
        This function will convert the labels back into their character form.

        Parameters:
            - _label: A list containing the label to convert

        Returns:
            The transcript of the corresponding labelc
        """
        return tf.strings.reduce_join(self.numToChar(_label)).numpy().decode("utf-8")

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
    
    def LoadAudioFile(self, _file):
        """
        This method transforms a given audio sample into a spectrogram using tensorflows stft method.
        It will normalize the sample before returning it.
        
        Parameters:
            - _file: The file path to a given audio sample

        Returns:
            A spectrogram
        """
        file = tf.io.read_file(_file)
        audio, _ = tf.audio.decode_wav(file)
        audio = tf.squeeze(audio, axis = -1)

        return audio

    # ------------------------------------------------------------------------
    #   Audio processing
    # ------------------------------------------------------------------------    
    def NormalizeSpec(self, _spectrogram):
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
    
    def Spectrogram(self, _audio):
        """
        This method will compute the Short-Time Fourier Transform (STFT) of a given audio sample.
        This method will convert the results into log form and normalize the results.

        Parameters:
            - _audio: The float tensor of the wav file

        Returns:
            The Spectrogram of a wav file
        """
        length  = int(self.spectrogramConfig['frame_length'])
        step    = int(self.spectrogramConfig['frame_step'])
        fft     = int(self.spectrogramConfig['fft'])

        audio = tf.cast(_audio, tf.float32)

        spectrogram = tf.signal.stft(audio, frame_length = length, frame_step = step, fft_length = fft)
        spectrogram = tf.abs(spectrogram)
        spectrogram = tf.math.pow(spectrogram, 0.5)

        return spectrogram

    # ------------------------------------------------------------------------
    #   Transcript processing
    # ------------------------------------------------------------------------
    def Transcript(self, _transcript):
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