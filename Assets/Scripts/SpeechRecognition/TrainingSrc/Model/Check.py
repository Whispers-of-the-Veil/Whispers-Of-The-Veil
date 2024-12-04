import tensorflow as tf
import numpy as np
from tensorflow.keras.models import Model, load_model
import h5py

import sys

from Model.ASRModel import ASRModel
from Data.Preporcess import Process

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python ValidateModel /path/to/model.keras /path/to/train.csv")
        exit(1)

    process = Process()

    print("Loading Test dataset")
    trainAudioPaths, trainTranscripts = process.LoadCSV(sys.argv[2])
    testSpectrogram, testLabel = process.Data(trainAudioPaths[0], trainTranscripts[0])
    
    model = load_model(sys.argv[1],  custom_objects = {'ctcLoss': ASRModel.ctcloss}, safe_mode = False)
    model.summary()

    # Predict the sentiment class for the spectrogram
    sentiment = model.predict(np.expand_dims(testSpectrogram, axis = 0))

    transcript = ASRModel.ctcDecoder(sentiment)
    transcript = process.ConvertLabel(transcript)

    print(f"Label{1}: {trainTranscripts[0]}\n")
    print(f"Prediction: {transcript}")