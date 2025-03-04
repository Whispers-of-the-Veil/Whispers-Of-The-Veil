import tensorflow as tf
import numpy as np

from Grab_Ini import ini

class Augment:
    def __init__(self):
        generalConfig    = ini().grabInfo("config.ini", "General")
        self.augmentConfig     = ini().grabInfo("config.ini", "Process.AugmentAudio")
        self.seed = int(generalConfig['seed'])

        tf.random.set_seed(self.seed)
        np.random.seed(self.seed)

    def Noise(self, _audio):
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
    
    def TimeStrech(self, _audio):
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

        factor = tf.random.uniform(shape = [], minval = min, maxval = max)

        length = tf.cast(tf.round(tf.cast(tf.shape(_audio)[0], tf.float32) / factor), tf.int32)

        return tf.signal.resample(_audio, length)
    
    def Volume(self, _audio):
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