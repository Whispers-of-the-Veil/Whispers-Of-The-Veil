import tensorflow as tf
import numpy as np
import h5py

from tensorflow.keras.models import load_model
from tensorflow.keras.callbacks import ReduceLROnPlateau, EarlyStopping

import sys
import os

from Model.ASRModel import ASRModel as asrmodel
from Config.Grab_Ini import ini

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
            loadedInfo.append(data[entry][:])

    return loadedInfo

def CreateDataset(_spectrograms, _Labels, _batchSize):
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
    dataSet = tf.data.Dataset.from_tensor_slices((_spectrograms, _Labels))
    dataSet = dataSet.shuffle(buffer_size = len(_spectrograms))
    dataSet = dataSet.batch(_batchSize)
    dataSet = dataSet.prefetch(tf.data.AUTOTUNE)

    return dataSet

def ctcLoss(_yTrue, _yPred):
    """
    CTC Loss using TensorFlow's `tf.nn.ctc_loss`.c

    Connectionist Temporal Classification (CTC) is used when the output sequences are shorter 
    than the input sequences (audio data). It allows the model to align predictions to target 
    labels without needing exact frame-level alignment.
    
    Parameters:
        - _yTrue: Ground truth labels (sparse representation).
        - _yPred: Model predictions (logits).

    Returns:
        Returns the mean of the computed CTC loss across the batch
    """
    batchSize = tf.shape(_yPred)[0]
    timeSteps = tf.shape(_yPred)[1]

    # Input and label lengths
    inputLength = tf.fill([batchSize], timeSteps)  # Length of the predictions
    labelLength = tf.reduce_sum(tf.cast(_yTrue != 0, tf.int32), axis=-1)  # Non-padded labels

    # Cast _yTrue to int32 if it is not already
    _yTrue = tf.cast(_yTrue, tf.int32)

    # Compute the CTC loss
    loss = tf.nn.ctc_loss(
        labels              =   _yTrue,
        logits              =   _yPred,
        label_length        =   inputLength,
        logit_length        =   labelLength,
        logits_time_major   =   False,
        blank_index         =   -1              # Last class used as the blank index
    )

    return tf.reduce_mean(loss)

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
    learingConfig   = ini().grabInfo("config.ini", "Training.LearningRate")
    stopConfig      = ini().grabInfo("config.ini", "Training.EarlyStop")

    seed            = int(generalConfig['seed'])
    batchSize       = int(trainingConfig['batch_size'])
    numEpochs       = int(trainingConfig['epochs'])
    learnMonitor    = str(learingConfig['monitor'])
    factor          = float(learingConfig['factor'])
    learnPatience   = int(learingConfig['patience'])
    minLR           = float(learingConfig['min_lr'])
    stopMonitor     = str(stopConfig['monitor'])
    stopPatience    = int(stopConfig['patience'])

    # Set the seed value for experiment reproducibility.
    tf.random.set_seed(seed)
    np.random.seed(seed)

    print("\nInitializing Data Sets...")

    shapes = LoadH5(sys.argv[1], ['InputShape', 'OutputSize'])

    inputShape = tuple(shapes[0])[1:]
    outputSize = tuple(shapes[1])[1:]

    print(f"InputShape: {inputShape} | OutputSize: {outputSize}")

    trainingInfo = LoadH5(sys.argv[1], ['Spectrograms', 'Labels'])
    trainDataSet = CreateDataset(trainingInfo[0], trainingInfo[1], batchSize)
    trainingInfo.clear
    
    validationInfo = LoadH5(sys.argv[2], ['Spectrograms', 'Labels'])
    validDataSet = CreateDataset(validationInfo[0], validationInfo[1], batchSize)
    validationInfo.clear

    # Load existing model if it exists
    if os.path.exists(sys.argv[3]):
        print(f"\nLoaded existing model {sys.argv[3]}")

        model = load_model(
            sys.argv[3], 
            custom_objects = {'ctcLoss': ctcLoss}, 
            safe_mode = False
        )
    else:
        print(f"\nBuilding a new model")

        model = asrmodel.BuildModel(inputShape, outputSize)

        model.compile(
            optimizer   = 'adam', 
            loss        = ctcLoss, 
            metrics     = ['accuracy']
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
    model.fit(
        trainDataSet,
        validation_data = validDataSet,
        epochs          = numEpochs,
        verbose         = 1,
        callbacks       = [reduce_lr, early_stopping]
    )

    model.save(sys.argv[3])
    model.summary()

    