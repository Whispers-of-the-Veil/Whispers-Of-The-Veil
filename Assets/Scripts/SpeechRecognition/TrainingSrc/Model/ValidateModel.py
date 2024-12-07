from tensorflow import keras
from jiwer import wer

from Model.ASRModel import ASRModel
from Data.Preporcess import Process

class ValidateModel(keras.callbacks.Callback):
    def __init__(self, _dataset):
        super().__init__()

        self.dataset = _dataset
        self.process = Process()

        self.errorRate = []

    def on_epoch_end(self, epoch, logs = None):
        """
        This function will compute the word error rate at the end of each epoch. The error rate
        will be added to a custom metric for the models logs; 'Error_Rate'.

        Parameters:
            - epoch: The epoch that just completed
            - logs: 
        """
        transcripts = []
        predictions = []

        model = self.model

        for batch in self.dataset:
            spectrogram, labels = batch

            output = model.predict(spectrogram)

            output = ASRModel.ctcDecoder(output)

            for item in output:
                decoded = self.process.ConvertLabel(item)

                predictions.append(decoded)

            for item in labels:
                item = self.process.ConvertLabel(item)
                transcripts.append(item)

        errorRate = wer(transcripts, predictions)

        # This will add a custom metric to the models logs for the error rate
        logs['Error_Rate'] = errorRate