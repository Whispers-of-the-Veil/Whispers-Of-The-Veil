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
    
    # Custom CTC Greedy Decoder for evaluation
    def ctc_greedy_decoder(y_pred):
        input_length = tf.fill([tf.shape(y_pred)[0]], tf.shape(y_pred)[1])
        decoded, _ = tf.nn.ctc_greedy_decoder(y_pred, input_length)
        return decoded

    def Predict(self, _audioSample, _sampleRate):
        logMelSpec = self._ComputeMelSpectrogram(_audioSample, _sampleRate)

        logMelSpec = np.expand_dims(logMelSpec, axis = 0).astype(np.float32)
        logMelSpec = np.expand_dims(logMelSpec, -1)

        self.interpreter.set_tensor(self.inputDetails[0]['index'], logMelSpec)
        self.interpreter.invoke()

        outputData = self.interpreter.get_tensor(self.outputDetails[0]['index'])

        

        return prediction
    