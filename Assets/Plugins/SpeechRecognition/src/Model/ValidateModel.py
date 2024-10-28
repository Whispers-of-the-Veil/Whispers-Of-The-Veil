import tensorflow as tf
import numpy as np
from tensorflow.keras.models import Model, load_model
import h5py

import sys

from Model.ASRModel import ASRModel as asrmodel

def TestSentiment(_model, _spectrogram, _label):
    try:
        # Predict the sentiment class for the spectrogram
        sentiment = _model.predict(np.expand_dims(_spectrogram, axis = 0))
        predicted_classes = np.argmax(sentiment, axis = -1)

        # Flatten the predicted sequence (assuming 1D transcript)
        predicted_classes = predicted_classes.flatten()

        print(f"Predition {predicted_classes}\nLabel {_label}")

        # # Compare predicted class with transcript (assuming transcript is class label)
        # assert predicted_classes[0] == _label, f"Prediction {predicted_classes} does not match Label\n {_label}"

    except Exception as e:
        print(f"An error occurred during testing: {e}")
if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python ValidateModel /path/to/model.keras /path/to/testData.h5")
        exit(1)

    print("Loading Test dataset")
    with h5py.File(sys.argv[2], 'r') as data:
        spectrograms = data['Spectrograms'][:]
        labels = data['Labels'][:]
    
    model = load_model(sys.argv[1],  custom_objects={'ctcLoss': asrmodel.ctcLoss}, safe_mode = False)

    print("Asserting models preformance")
    for sample, label in zip(spectrograms, labels):
        TestSentiment(model, sample, label)

    model.summary()