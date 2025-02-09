import sys
import os
import numpy as np
from tensorflow.keras.models import load_model

from Data.NLP import NLP
from Data.Process import Process
from Model.ASRModel import ASRModel
from ModelInterface import Interface

class SpeechRec(Interface):
    def __init__(self):
        if getattr(sys, 'frozen', False):
            scriptDir = sys._MEIPASS 
        else:
            scriptDir = os.path.dirname(os.path.abspath(__file__))

        filePath = os.path.join(scriptDir, 'models', 'ASR.keras')

        self.model = load_model(filePath, custom_objects = {'ctcloss': ASRModel.ctcloss}, safe_mode = False)
        self.process = Process()
        self.NLP = NLP()

    def Predict(self, _audio):
        """
        Generate a prediction from a given model
        
        Parameters:
            - value: np.float23

        Returns:
        A string representing the prediction
        """
        spectrogram = np.expand_dims(self.process._Spectrogram(_audio), axis = 0)

        sentiment = self.model.predict(spectrogram)
        softmax = ASRModel.ctcDecoder(sentiment)
        prediction = self.process.ConvertLabel(softmax)

        print(prediction)

        return prediction

    def PostProcess(self, _prediction):
        """
        Preform post processing on the models prediction to clean up the models
        output.

        Parameters:
            - _prediction: A string of the prediction generated from the model

        returns:
            The cleaned up prediction
        """
        prediction = self.NLP.SpellingCorrection(_prediction)

        return prediction