# Lucas Davis

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
        generalConfig          = ini().grabInfo("config.ini", "General")
        self.processConfig     = ini().grabInfo("config.ini", "Process")
        self.augmentConfig     = ini().grabInfo("config.ini", "Process.AugmentAudio")
        self.spectrogramConfig = ini().grabInfo("config.ini", "Process.Spectrogram")
        labelConfig            = ini().grabInfo("config.ini", "Process.Label")

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
    
    def TrainData(self, _audio, _transcript):
        """
        This method is used when we are maping the audio paths and transcripts to their
        spectrograms and labels when creating a dataset in the Training script. It simply calls
        the _Audio and _Transcript methods and returns the spectrogram and label of the audio sample.

        This method will utilize a different method then Data that will augment the audio samples before
        converting them into a spectrogram.

        Parameters:
            - _audio: The path of the audio file; must be a .wav file
            - _transcript: The transcript of the audio file

        Returns:
            The spectrogram and label of the audio sample
        """
        spectrogram = self.TrainAudio(_audio)
        label = self._Transcript(_transcript)
            
        return spectrogram, label
    
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
        file = tf.io.read_file(_file)
        audio, _ = tf.audio.decode_wav(file)
        audio = tf.squeeze(audio, axis = -1)

        audio = tf.cast(audio, tf.float32)

        return self._Spectrogram(audio)

    def TrainAudio(self, _file):
        """
        This method is similar to the Audio method, however this method will augment the training set
        if the augment value is True in the ini file.

        Parameters:
            - _file: The file path to a given audio sample

        Returns:
            A spectrogram
        """
        file = tf.io.read_file(_file)
        audio, _ = tf.audio.decode_wav(file)
        audio = tf.squeeze(audio, axis = -1)

        if eval(self.processConfig['augment']):
            audio = self._Noise(audio)
            audio = self._Volume(audio)

        audio = tf.cast(audio, tf.float32)

        return self._Spectrogram(audio)
    
    def _Noise(self, _audio):
        """
        This function will preform additive noise with a uniform distribution in range of a 
        defined min and max value. These values can be configured in the ini file.

        Parameters:
            - _audio: The float tensor of the wav file

        Returns:
            The augmented audio file with additive noise
        """
        min = -(int(self.augmentConfig['noise_min']))
        max = -(int(self.augmentConfig['noise_max']))

        if min == 0 or max == 0:
            return _audio
        
        noiseLevel = tf.random.uniform(shape = [], minval = min, maxval = max, seed = self.seed)
        noise = tf.random.uniform(tf.shape(_audio)) * tf.math.pow(10.0, noiseLevel / 20.0)

        return _audio + noise
    
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
    
    def _Spectrogram(self, _audio):
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

        spectrogram = tf.signal.stft(_audio, frame_length = length, frame_step = step, fft_length = fft)
        spectrogram = tf.abs(spectrogram)
        spectrogram = tf.math.pow(spectrogram, 0.5)

        return self._Normalize(spectrogram)

    def _TimeStretch(self, _audio):
        """
        This mehtod preforms Time Streching by resampling the audio based on a uniform distribution. This will alter the
        tempo of the speaker, making them speak faster or slower. The ratio is defined in the ni file.

        Parameters:
            - _audio: The float tensor of the wav file

        Returns:
            The augmented audio file after its tempo is streched or compressed
        """
        strechRatio = int(self.augmentConfig['time_stretch_ratio'])

        if strechRatio == 0:
            return _audio

        min = 1.0 - (strechRatio / 100.0)
        max = 1.0 + (strechRatio / 100.0)

        factor = tf.random.uniform(shape=[], minval = min, maxval = max)

        length = tf.cast(tf.round(tf.cast(tf.shape(_audio)[0], tf.float32) / factor), tf.int32)

        return tf.signal.resample(_audio, length)
    
    def _Volume(self, _audio):
        """
        This method will scale the amplitude of the audio sample based on a uniform distribution. This will
        change the volume of the audio samples, making the speaker loader or quieter based on a ratio in the ini
        file.

        Parameters:
            - _audio: The float tensor of the wav file

        Returns:
            The augmented audio file with its volume ajusted
        """
        volRatio = int(self.augmentConfig['volume_ratio'])

        if volRatio == 0:
            return _audio
        
        min = 1 - (volRatio / 100)
        max = 1 + (volRatio / 100)
        
        volume = tf.random.uniform(shape = [], minval = min, maxval = max)

        return _audio * volume

    # ------------------------------------------------------------------------
    #   Transcript processing
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