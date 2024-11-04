import tensorflow as tf
import numpy as np
import matplotlib.pyplot as plt

from tensorflow.keras.models import load_model
from tensorflow.keras.callbacks import ReduceLROnPlateau, EarlyStopping
from tensorflow.keras.optimizers import Adam

import sys
import os

from Model.ASRModel import ASRModel as asrmodel
from Config.Grab_Ini import ini
from Data.Preporcess import Process

def PlotLoss(_history):
    """
    This function plots the loss and val_loss that was recorred over each epoch.
    Used to help determine the preformance of the model on a provided Training and
    Validation set.

    Parameters:
        - _history: This is the history of the model during training
    """
    metrics = _history.history

    # Plot the loss and val loss per each Epoch
    plt.figure(figsize=(16,6))
    plt.plot(_history.epoch, metrics['loss'], metrics['val_loss'])
    plt.legend(['loss', 'val_loss'])
    plt.ylim([0, max(plt.ylim())])
    plt.xlabel('Epoch')
    plt.ylabel('Loss [ctcloss]')
    plt.show()

def CreateDataset(_spectrograms, _labels, _batchSize):
    """
    Creates a TensorFlow dataset from pairs of audio data and transcript data, 
    shuffles the data, batches it, and optimizes it for processing.

    Parameters:
        - _spectrograms: An array of Mel Spectrograms
        - _trainscriptData: An array of transcript data for the audio samples
        - _batchSize: Defines the size of the batches that the dataset will be divided by

    Returns:
        Returns a Tensorflow dataset
    """
    # Ensure that the lengths are numpy arrays
    dataSet = tf.data.Dataset.from_tensor_slices((_spectrograms, _labels))
    dataSet = dataSet.shuffle(buffer_size = len(_spectrograms))
    dataSet = dataSet.batch(_batchSize)
    dataSet = dataSet.prefetch(tf.data.AUTOTUNE)

    return dataSet

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("usage: python SpeechRecognition.py /Path/to/TrainingData.csv /Path/to/ValidationData.csv /output/path/to/the/model.keras")
        print("If you haven't please run the Preprocess script on the data")
        print("\nThe arguments are:")
        print(" - The path to the directory containing the batches of Training Data")
        print(" - The path to the directory containing the batches of Validation Data")
        print(" - The path to an existing model, or the path to save the model")

        exit(1)

    print("Loading Config")
    generalConfig   = ini().grabInfo("config.ini", "General")
    trainingConfig  = ini().grabInfo("config.ini", "Training")
    learningConfig  = ini().grabInfo("config.ini", "Training.LearningRate")
    stopConfig      = ini().grabInfo("config.ini", "Training.EarlyStop")

    seed            = int(generalConfig['seed'])

    batchSize       = int(trainingConfig['batch_size'])
    numEpochs       = int(trainingConfig['epochs'])

    learnMonitor    = str(learningConfig['monitor'])
    factor          = float(learningConfig['factor'])
    learnPatience   = int(learningConfig['patience'])
    lr              = float(learningConfig['learning_rate'])
    minLR           = float(learningConfig['min_lr'])

    # Set the seed value for experiment reproducibility.
    tf.random.set_seed(seed)
    np.random.seed(seed)

    process = Process()

    print(f"\nProcessing {sys.argv[1]}...")
    trainAudioPaths, trainTranscripts = process.LoadCSV(sys.argv[1])
    trainSpectrograms, trainSpecShape = process.Audio(trainAudioPaths, 0)
    trainLabels, trainNumClasses = process.Transcript(trainTranscripts, 0)

    # process.ValidateData(trainSpectrograms, trainLabels, 3)
    
    print("Creating Dataset")
    trainDataSet = CreateDataset(trainSpectrograms, trainLabels, batchSize)
    del trainSpectrograms, trainLabels

    print(f"\nProcessing {sys.argv[2]}...")
    validAudioPaths, validTranscripts = process.LoadCSV(sys.argv[2])
    validSpectrograms, validSpecShape = process.Audio(validAudioPaths, 0, True)
    validLabels, validNumClasses = process.Transcript(validTranscripts, 0, True)

    print("Creating Dataset")
    validDataSet = CreateDataset(validSpectrograms, validLabels, batchSize)
    del validSpecShape, validNumClasses, validSpectrograms, validLabels

    if os.path.exists(sys.argv[3]):
        print(f"\nLoaded existing model {sys.argv[3]}")

        model = load_model(
            sys.argv[3], 
            custom_objects = {'ctcLoss': asrmodel.ctcLoss}, 
            safe_mode = False
        )
    else:
        print(f"\nBuilding a new model")

        model = asrmodel.BuildModel(trainSpecShape, trainNumClasses)

        model.compile(
            optimizer   = Adam(learning_rate = lr), 
            loss = asrmodel.ctcLoss
        )

    reduce_lr = ReduceLROnPlateau(
        monitor     = learnMonitor, 
        factor      = factor, 
        patience    = learnPatience, 
        min_lr      = minLR
    )

    model.summary()

    print("\nTraining Model...")
    history = model.fit(
        trainDataSet,
        validation_data = validDataSet,
        epochs          = numEpochs,
        validation_split=0.2,
        callbacks       = [reduce_lr]
    )

    PlotLoss(history)

    model.save(sys.argv[3])
    model.summary()