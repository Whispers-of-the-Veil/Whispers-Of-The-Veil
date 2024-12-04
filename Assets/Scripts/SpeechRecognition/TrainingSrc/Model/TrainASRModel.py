import os
import sys
import numpy as np
import matplotlib.pyplot as plt

import tensorflow as tf
from tensorflow.keras.models import load_model
from tensorflow.keras.optimizers import Adam

from Model.ASRModel import ASRModel
from Grab_Ini import ini
from Data.Preporcess import Process
from Model.ValidateModel import ValidateModel

def PlotErrorRate(_history):
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
    plt.show()

def PlotLoss(_history):
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
    plt.show()

def Debug(process: Process, _train, _valid):
    """
    
    """
    for spectrogram, label in _train.take(1):
        process.ValidateData(spectrogram[0], label[0])

    for spectrogram, label in _valid.take(1):
        process.ValidateData(spectrogram[0], label[0])


def CreateDataset(process: Process, _training, _validation):
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

    trainDataset = tf.data.Dataset.from_tensor_slices(
        (list(trainAudioPaths), list(trainTranscripts))
    )
    trainDataset = (
        trainDataset.map(process.Data, num_parallel_calls = tf.data.AUTOTUNE)
        .padded_batch(batchSize)
        .prefetch(buffer_size = tf.data.AUTOTUNE)
    )

    validationData = tf.data.Dataset.from_tensor_slices(
        (list(validAudioPaths), list(validTranscripts))
    )
    validationData = (
        validationData.map(process.Data, num_parallel_calls = tf.data.AUTOTUNE)
        .padded_batch(batchSize)
        .prefetch(buffer_size = tf.data.AUTOTUNE)
    )

    return trainDataset, validationData

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("usage: python TrainASRModel.py /Path/to/TrainingData.csv /Path/to/ValidationData.csv /output/path/to/the/model.keras")
        exit(1)

    print("Loading Config")
    generalConfig       = ini().grabInfo("config.ini", "General")
    trainingConfig      = ini().grabInfo("config.ini", "Training")
    learningConfig      = ini().grabInfo("config.ini", "Training.LearningRate")
    preprocessConfig    = ini().grabInfo("config.ini", "Preprocess")

    seed            = int(generalConfig['seed'])
    batchSize       = int(trainingConfig['batch_size'])
    numEpochs       = int(trainingConfig['epochs'])
    lr              = float(learningConfig['learning_rate'])
    decaySteps      = int(learningConfig['decay_steps'])
    decayRate       = float(learningConfig['decay_rate'])
    display         = int(preprocessConfig['display_samples']) != 0
    fft             = int(preprocessConfig['fft'])
    
    tf.random.set_seed(seed)
    np.random.seed(seed)

    process = Process()

    expDecayLR = tf.keras.optimizers.schedules.ExponentialDecay(
        lr,
        decay_steps = decaySteps,
        decay_rate = decayRate,
        staircase = True
    )

    if os.path.exists(sys.argv[3]):
        print(f"\nLoaded existing model {sys.argv[3]}")

        model = load_model(
            sys.argv[3], 
            custom_objects = {'ctcLoss': ASRModel.ctcloss}, 
            safe_mode = False
        )
    else:
        print(f"\nBuilding a new model")

        model = ASRModel.BuildModel(
            fft // 2 + 1,                           # Input size
            process.charToNum.vocabulary_size()     # Num of Classes
        )

        model.compile(
            optimizer   = Adam(learning_rate = expDecayLR), 
            loss = ASRModel.ctcloss
        )

    model.summary()

    trainDataset, validationData = CreateDataset(process, sys.argv[1], sys.argv[2])

    validate = ValidateModel(validationData)

    if display:
        Debug(process, trainDataset, validationData)

    history = model.fit(
        trainDataset,
        validation_data  = validationData,
        epochs           = numEpochs,
        callbacks        = [validate]
    )

    PlotLoss(history)
    PlotErrorRate(history)

    model.save(sys.argv[3])
    model.summary()