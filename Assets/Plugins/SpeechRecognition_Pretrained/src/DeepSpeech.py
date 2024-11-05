import tensorflow as tf
import numpy as np
import librosa

class API:
    def __init__(self):
        self.interpreter = tf.lite.Interpreter(model_path = "../Model/ASRModel.tflite")
        self.interpreter.allocate_tensors()

        self.inputDetails = self.interpreter.get_input_details()
        self.outputDetails = self.interpreter.get_output_details()

    def _ComputeMelSpectrogram(self, _audioSample, _sampleRate):
        targetLength    = int(5 * _sampleRate) - 1
        windowLength    = int(0.025 * _sampleRate)
        hopLength       = int(0.010 * _sampleRate)

        # Extend the Mel Frequency bins to 500; the model is expecting that input shape
        if len(_audioSample) < targetLength:
            padding = targetLength - len(_audioSample)
            _audioSample = np.concatenate((_audioSample, np.zeros(padding, dtype=np.float32)))
        elif len(_audioSample) > targetLength:
            _audioSample = _audioSample[:targetLength]

        # Compute the Mel spectrogram in the Log-scale of the audio sample
        spectrogram = librosa.stft(_audioSample, n_fft = windowLength, hop_length = hopLength)
        
        magnitude, _ = librosa.magphase(spectrogram)
        melScaleSpectrogram = librosa.feature.melspectrogram(
            S = magnitude,
            sr = _sampleRate,
            n_fft = windowLength,
            hop_length = hopLength
        )
        
        logMelSpectrogram = librosa.amplitude_to_db(melScaleSpectrogram, ref = np.max)

        return logMelSpectrogram
    
    def _Decode(self, _output):
        charMap = {
            ' ': 1,
            'a': 2, 
            'b': 3, 
            'c': 4, 
            'd': 5, 
            'e': 6, 
            'f': 7, 
            'g': 8, 
            'h': 9, 
            'i': 10, 
            'j': 11, 
            'k': 12, 
            'l': 13, 
            'm': 14, 
            'n': 15, 
            'o': 16, 
            'p': 17, 
            'q': 18, 
            'r': 19, 
            's': 20, 
            't': 21, 
            'u': 22, 
            'v': 23, 
            'w': 24, 
            'x': 25, 
            'y': 26, 
            'z': 27, 
            '<PAD>': 0
        }
        charIndices = np.argmax(_output, axis=2)

        prediction = ''.join([charMap[index] for index in charIndices[0]]) 

        return prediction

    def Predict(self, _audioSample, _sampleRate):
        logMelSpec = self._ComputeMelSpectrogram(_audioSample, _sampleRate)

        logMelSpec = np.expand_dims(logMelSpec, axis = 0).astype(np.float32)
        logMelSpec = np.expand_dims(logMelSpec, -1)

        self.interpreter.set_tensor(self.inputDetails[0]['index'], logMelSpec)
        self.interpreter.invoke()

        outputData = self.interpreter.get_tensor(self.outputDetails[0]['index'])

        prediction = self._Decode(outputData)

        return prediction
    