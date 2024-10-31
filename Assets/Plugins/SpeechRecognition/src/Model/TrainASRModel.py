import tensorflow as tf
import numpy as np
import h5py
import matplotlib.pyplot as plt

from tensorflow.keras.models import load_model
from tensorflow.keras.callbacks import ReduceLROnPlateau, EarlyStopping
from tensorflow.keras.optimizers import Adam



import sys
import os

from Model.ASRModel import ASRModel as asrmodel
from Config.Grab_Ini import ini

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

def LoadH5(_file, _data):
    """
    Load in the information from the _data list from the h5 file

    Parameters:
        - _file: the complete file path to the h5 file
        - _data: A list containing strings that identify what to pull out

    Returns:
        Returns a list of the pulled information
    """
    loadedInfo = []

    with h5py.File(_file, 'r') as data:
        for entry in _data:
            if data[entry].ndim == 0:  # scalar dataset
                loadedInfo.append(data[entry][()])
            else:  # array dataset
                loadedInfo.append(data[entry][:])

    return loadedInfo

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("usage: python SpeechRecognition.py /Path/to/TrainingData/Directory /Path/to/ValidationData/Directory /output/path/to/the/model.keras")
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
    stopMonitor     = str(stopConfig['monitor'])
    stopPatience    = int(stopConfig['patience'])

    # Set the seed value for experiment reproducibility.
    tf.random.set_seed(seed)
    np.random.seed(seed)

    print("\nInitializing Data Sets...")

    shapes = LoadH5(sys.argv[1], ['InputShape', 'OutputSize'])

    inputShape = tuple(shapes[0])[1:]
    outputSize = shapes[1]

    print(f"InputShape: {inputShape} | OutputSize: {outputSize}")

    trainingInfo = LoadH5(sys.argv[1], ['MFCC', 'Labels'])
    trainDataSet = CreateDataset(trainingInfo[0], trainingInfo[1], batchSize)

    inputLengths = np.full((shapes[0][0],), shapes[0][2], dtype=int)
    labelLengths = np.array([np.count_nonzero(label) for label in trainingInfo[1]])

    trainingInfo.clear
    
    validationInfo = LoadH5(sys.argv[2], ['MFCC', 'Labels'])
    validDataSet = CreateDataset(validationInfo[0], validationInfo[1], batchSize)
    validationInfo.clear

    # Load existing model if it exists
    if os.path.exists(sys.argv[3]):
        print(f"\nLoaded existing model {sys.argv[3]}")

        model = load_model(
            sys.argv[3], 
            custom_objects = asrmodel.ctcLoss, 
            safe_mode = False
        )
    else:
        print(f"\nBuilding a new model")

        model = asrmodel.BuildModel(inputShape, outputSize)

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
    
    early_stopping = EarlyStopping(
        monitor = stopMonitor, 
        patience = stopPatience, 
        restore_best_weights = True
    )

    model.summary()

    print("\nTraining Model...")
    history = model.fit(
        trainDataSet,
        validation_data = validDataSet,
        epochs          = numEpochs,
        verbose         = 1,
        callbacks       = [reduce_lr, early_stopping]
    )

    PlotLoss(history)

    model.save(sys.argv[3])
    model.summary()

    