import tensorflow as tf
import numpy as np
from tensorflow.keras.models import Model, load_model

import sys

from Model.ASRModel import ASRModel
from Data.Preporcess import Process

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python ValidateModel /path/to/model.keras /path/to/train.csv")
        exit(1)

    process = Process()

    print("Loading Test dataset")
    audioPaths, transcripts = process.LoadCSV(sys.argv[2])
    
    model = load_model(sys.argv[1],  custom_objects = {'ctcloss': ASRModel.ctcloss}, safe_mode = False)
    model.summary()

    for i in range(len(audioPaths)):
        testSpectrogram, testLabel = process.Data(audioPaths[i], transcripts[i])

        # Predict the sentiment class for the spectrogram
        sentiment = model.predict(np.expand_dims(testSpectrogram, axis = 0))

        prediction = ASRModel.ctcDecoder(sentiment)
        prediction = process.ConvertLabel(prediction)

        print(f"Label{i}: {transcripts[i]}\n")
        print(f"Prediction: {prediction}")