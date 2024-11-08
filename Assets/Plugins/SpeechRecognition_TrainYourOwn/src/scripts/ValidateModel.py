import tensorflow as tf
import numpy as np
from tensorflow.keras.models import Model, load_model
import h5py

import sys

from Model.ASRModel import ASRModel as asrmodel
from Data.Preporcess import Process

def TestSentiment(_model, _spectrogram, _label):
    try:
        # Predict the sentiment class for the spectrogram
        sentiment = _model.predict(np.expand_dims(_spectrogram, axis = 0))
        predicted_classes = np.argmax(sentiment, axis = -1)

        print(f"Predition {predicted_classes}\nLabel {_label}")

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
    
    model = load_model(sys.argv[1],  custom_objects={'ctcLoss': asrmodel.ctcLoss}, safe_mode = False)

    # Predict the sentiment class for the spectrogram
    sentiment = model.predict(np.expand_dims(testSpectrograms[0], axis = 0))

    # Flatten the predicted sequence (assuming 1D transcript)

    print(f"Shape of prediction {sentiment.shape}")

    print(f"Predition {sentiment}\nLabel {testLabels[0]}")

    # print("Asserting models preformance")
    # for sample, label in zip(testSpectrograms, testLabels):
    #     TestSentiment(model, sample, label)

    model.summary()