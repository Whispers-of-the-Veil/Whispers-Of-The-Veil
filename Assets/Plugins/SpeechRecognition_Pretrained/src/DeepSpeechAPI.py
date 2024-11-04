import tensorflow as tf
import numpy as np

class DeepSpeechAPI:
    def __init__(self):
        self.interpreter = tf.lite.Interpreter(model_path = "../Model/ASRModel.tflite")
        self.interpreter.allocate_tensors()

        self.inputDetails = self.interpreter.get_input_details()
        self.outputDetails = self.interpreter.get_output_details()

    def Predict(self, _mfccs):
        if _mfccs is None:
            raise ValueError("MFCCs weren't provided")

        _mfccs = np.expand_dims(_mfccs, axis = 0).astype(np.float32)
        print(_mfccs.shape)
        _mfccs = np.expand_dims(_mfccs, -1)
        print(_mfccs.shape)

        self.interpreter.set_tensor(self.inputDetails[0]['index'], _mfccs)
        self.interpreter.invoke()

        outputData = self.interpreter.get_tensor(self.outputDetails[0]['index'])

        return outputData