import tensorflow as tf
import numpy as np
from tensorflow.keras.models import Model, load_model
import h5py

import sys

from Model.ASRModel import ASRModel
from Data.Preporcess import Process

def TestSentiment(_model, _spectrogram, _label):
    try:
        # Predict the sentiment class for the spectrogram
        sentiment = _model.predict(np.expand_dims(_spectrogram, axis = 0))
        predicted_classes = np.argmax(sentiment, axis = -1)
        

        print(f"Predition {transcript}\nLabel {_label}")

        # # Compare predicted class with transcript (assuming transcript is class label)
        # assert predicted_classes[0] == _label, f"Prediction {predicted_classes} does not match Label\n {_label}"

    except Exception as e:
        print(f"An error occurred during testing: {e}")
if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python ValidateModel /path/to/model.keras /path/to/train.csv")
        exit(1)

    process = Process()

    print("Loading Test dataset")
    trainAudioPaths, trainTranscripts = process.LoadCSV(sys.argv[2])
    testSpectrograms = process.Audio(trainAudioPaths, 0)
    testLabels = process.Transcript(trainTranscripts, 0)
    
    model = load_model(sys.argv[1],  custom_objects = {'ctcLoss': ASRModel.ctcloss}, safe_mode = False)
    model.summary()

    # Predict the sentiment class for the spectrogram
    sentiment = model.predict(np.expand_dims(testSpectrograms[0], axis = 0))

    transcript = ASRModel.ctcDecoder(sentiment)

    print(testLabels[0])