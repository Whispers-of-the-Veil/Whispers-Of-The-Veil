import tensorflow as tf
import numpy as np

from tensorflow.keras import backend as K

import librosa


class API:
    def __init__(self):
        self.interpreter = tf.lite.Interpreter(model_path = "../Model/ASRModel.tflite")
        self.interpreter.allocate_tensors()

        self.inputDetails = self.interpreter.get_input_details()
        self.outputDetails = self.interpreter.get_output_details()

    def _ComputeMelSpectrogram(self, _audioSample, _sampleRate):
        """
        Read an audio file, compute its Mel spectrogram, normalize it, and pad/truncate it to the target length.

        Parameters:

        Returns:
            The normalized Mel spectrogram for the given audio file
        """
        windowlength = int(0.025 * _sampleRate)
        hopLength = int(0.010 * _sampleRate)

        targetLength = int(5 * _sampleRate) - 1
        if len(_audioSample) < targetLength:
            padding = targetLength - len(_audioSample)
            _audioSample = np.concatenate((_audioSample, np.zeros(padding, dtype=np.float32)))
        elif len(_audioSample) > targetLength:
            _audioSample = _audioSample[:targetLength]  # Truncate if necessary

        spectrogram = librosa.feature.melspectrogram(y = _audioSample, sr = _sampleRate, n_mels = 128, hop_length = hopLength, win_length = windowlength)
        logSpectrogram = librosa.power_to_db(spectrogram, ref = np.max)

        return self._Normalize(logSpectrogram)
    
    def _Normalize(self, _melSpectrogram):
        """
        Normalizes the Mel spectrogram using z-score normalization.
        
        Parameters:
            - _melSpectrogram: The computed Mel spectrogram (2D NumPy array)
            
        Returns:
            - Normalized spectrogram with mean 0 and standard deviation 1
        """
        # Center the spectrogram by subtracting the mean
        centeredSpectrogram = _melSpectrogram - np.mean(_melSpectrogram)
        
        # Scale the centered spectrogram to [-1, 1] by dividing by the max absolute value
        maxAbsValue = np.max(np.abs(centeredSpectrogram))
        normalizedSpectrogram = centeredSpectrogram / maxAbsValue if maxAbsValue != 0 else centeredSpectrogram
        
        return normalizedSpectrogram
    
    def ctcGreedyDecoder(self, logits):
        print(f"logits shape: {logits.shape}")
        print(f"Logits\n{logits}")

        input_length = np.ones(logits.shape[0]) * logits.shape[1] 
        decoded, _ = K.ctc_decode(logits, input_length, greedy=True)

        print(f"Decoded: {decoded}")

        return K.get_value(decoded[0])

    def Predict(self, _audioSample, _sampleRate):
        logMelSpec = self._ComputeMelSpectrogram(_audioSample, _sampleRate)

        logMelSpec = np.expand_dims(logMelSpec, axis = 0).astype(np.float32)
        logMelSpec = np.expand_dims(logMelSpec, -1)

        self.interpreter.set_tensor(self.inputDetails[0]['index'], logMelSpec)
        self.interpreter.invoke()

        outputData = self.interpreter.get_tensor(self.outputDetails[0]['index'])

        print(outputData)

        decoded = self.ctcGreedyDecoder(outputData)

        print(decoded)       

        return decoded
    