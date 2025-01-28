# Lucas Davis

import os
import sys
import numpy as np
import matplotlib.pyplot as plt

import tensorflow as tf
from tensorflow.keras.models import load_model
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.callbacks import ModelCheckpoint, EarlyStopping
from tensorflow.keras.optimizers.schedules import ExponentialDecay

from Model.ASRModel import ASRModel
from Grab_Ini import ini
from Data.Process import Process
from Model.ValidateModel import ValidateModel

def PlotErrorRate(_history, _path):
    """
    This function plots the error rate of the model that was recorded from the
    callback class ValidateModel.

    Parameters:
        - _rate: A list of error rates
        - _history: 
    """
    metrics = _history.history

    plt.figure(figsize = (16, 6))
    plt.plot(_history.epoch, metrics['Error_Rate'])
    plt.legend('Error Rate')
    plt.ylim([0, max(plt.ylim())])
    plt.xlabel('Epoch')
    plt.ylabel('Error Rate')
    plt.savefig(_path + 'ErrorRate.png')

def PlotLoss(_history, _path):
    """
    This function plots the loss and val_loss that was recorred over each epoch.
    Used to help determine the preformance of the model on a provided Training and
    Validation set.

    Parameters:
        - _history: This is the history of the model during training
    """
    metrics = _history.history

    plt.figure(figsize = (16, 6))
    plt.plot(_history.epoch, metrics['loss'], metrics['val_loss'])
    plt.legend(['loss', 'val_loss'])
    plt.ylim([0, max(plt.ylim())])
    plt.xlabel('Epoch')
    plt.ylabel('Loss [ctcloss]')
    plt.savefig(_path + 'Loss.png')

def Debug(process: Process, _train, _valid, _test):
    """
    This function will display the labels and spcetrograms using process' ValidateData method.
    This is used to debug the spectrograms and the labels if there is a problem

    Parameters:
        - process: A reference to the process class
        - _train: The training dataset
        - _valid: The validation dataset
        - _test: The testing dataset
    """
    for spectrogram, label in _train.take(1):
        process.ValidateData(spectrogram[0], label[0])

    for spectrogram, label in _valid.take(1):
        process.ValidateData(spectrogram[0], label[0])

    for spectrogram, label in _test.take(1):
        process.ValidateData(spectrogram[0], label[0])

def CreateDataset(process: Process, _training, _validation, _test):
    """
    Creates a dataset for both the training and validatation sets. It will map the audio paths and transcripts
    to the process.Data method to process them into spectrograms and labels.

    Parameters:
        - _training: The path to the training data csv file
        - _validation: The path to the validation data csv file

    Returns:
        Two tensorflow datasets containing the training and validation sets
    """
    trainAudioPaths, trainTranscripts = process.LoadCSV(_training)
    validAudioPaths, validTranscripts = process.LoadCSV(_validation)
    testAudioSamples, testTranscripts = process.LoadCSV(_test)

    # The train dataset calls a different method that will augment the audio samples
    trainDataset = tf.data.Dataset.from_tensor_slices(
        (list(trainAudioPaths), list(trainTranscripts))
    )
    trainDataset = (
        trainDataset.map(process.TrainData, num_parallel_calls = tf.data.AUTOTUNE)
        .padded_batch(batchSize)
        .prefetch(buffer_size = tf.data.AUTOTUNE)
    )

    # The validation and testing datasets arent augmented. This is to keep them the same
    # between different training sets so we can get an accurate val_loss and Error_Rate
    # measurement
    validationData = tf.data.Dataset.from_tensor_slices(
        (list(validAudioPaths), list(validTranscripts))
    )
    validationData = (
        validationData.map(process.Data, num_parallel_calls = tf.data.AUTOTUNE)
        .padded_batch(batchSize)
        .prefetch(buffer_size = tf.data.AUTOTUNE)
    )

    testDataset = tf.data.Dataset.from_tensor_slices(
        (list(testAudioSamples), list(testTranscripts))
    )
    testDataset = (
        testDataset.map(process.Data, num_parallel_calls = tf.data.AUTOTUNE)
        .padded_batch(batchSize)
        .prefetch(buffer_size = tf.data.AUTOTUNE)
    )

    return trainDataset, validationData, testDataset

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("usage: python TrainASRModel.py /Path/to/TrainingData.csv /Path/to/ValidationData.csv /Path/to/TestData.csv")
        exit(1)

    print("Loading Config")
    generalConfig    = ini().grabInfo("config.ini", "General")
    trainingConfig   = ini().grabInfo("config.ini", "Training")
    learningConfig   = ini().grabInfo("config.ini", "Training.LearningRate")
    stopConfig       = ini().grabInfo("config.ini", "Training.EarlyStopping")
    processConfig    = ini().grabInfo("config.ini", "Process")
    spectrogramConfig= ini().grabInfo("config.ini", "Process.Spectrogram")

    seed             = int(generalConfig['seed'])
    batchSize        = int(trainingConfig['batch_size'])
    numEpochs        = int(trainingConfig['epochs'])
    pathToModel      = str(trainingConfig['path_to_model'])
    pathToCheckpoint = str(trainingConfig['path_to_checkpoint'])
    pathToFigures    = str(trainingConfig['path_to_figures'])
    lr               = float(learningConfig['learning_rate'])
    decaySteps       = int(learningConfig['decay_steps'])
    decayRate        = float(learningConfig['decay_rate'])
    stoppingPatients = int(stopConfig['patience'])
    debug            = eval(processConfig['display_spectrograms'])
    fft              = int(spectrogramConfig['fft'])
    
    tf.random.set_seed(seed)
    np.random.seed(seed)

    process = Process()

    expDecayLR = ExponentialDecay (
        lr,
        decay_steps = decaySteps,
        decay_rate  = decayRate,
        staircase   = True
    )

    if os.path.exists(pathToModel):
        print(f"\nLoaded existing model")

        model = load_model(
            pathToModel, 
            custom_objects = {'ctcloss': ASRModel.ctcloss}, 
            safe_mode = False
        )
    else:
        print(f"\nBuilding a new model")

        model = ASRModel.BuildModel (
            fft // 2 + 1,                           # Input size
            process.charToNum.vocabulary_size()     # Num of Classes
        )

        model.compile(
            optimizer   = Adam(learning_rate = expDecayLR), 
            loss = ASRModel.ctcloss
        )

    model.summary()

    trainDataset, validationData, testDataset = CreateDataset(process, sys.argv[1], sys.argv[2], sys.argv[3])

    validate = ValidateModel(testDataset)

    checkpoint = ModelCheckpoint (
        pathToCheckpoint + "BestWeights.keras",
        monitor        = 'Error_Rate',
        save_best_only = True,
        mode           = 'min',
        verbose        = 1
    )

    earlyStop = EarlyStopping (
        monitor              = 'val_loss',
        patience             = stoppingPatients,
        restore_best_weights = True
    )

    if debug:
        Debug(process, trainDataset, validationData, testDataset)

    history = model.fit (
        trainDataset,
        validation_data = validationData,
        epochs          = numEpochs,
        callbacks       = [validate, checkpoint, earlyStop]
    )

    PlotLoss(history, pathToFigures)
    PlotErrorRate(history, pathToFigures)

    model.save(pathToModel)
    model.summary()