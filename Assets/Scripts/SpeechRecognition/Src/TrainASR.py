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
from Data.Augment import Augment
from Data.Validate import Validate
from Model.ValidateModel import ValidateModel
from Model.Setup import Setup

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

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("usage: python TrainASRModel.py /Path/to/TrainingData.csv /Path/to/ValidationData.csv /Path/to/TestData.csv")
        exit(1)

    process = Process()
    augment = Augment()
    validate = Validate(process)
    setup = Setup(process, augment, validate)

    print("Loading Config")

    generalConfig    = ini().grabInfo("config.ini", "General")
    trainingConfig   = ini().grabInfo("config.ini", "Training")
    learningConfig   = ini().grabInfo("config.ini", "Training.LearningRate")
    stopConfig       = ini().grabInfo("config.ini", "Training.EarlyStopping")
    processConfig    = ini().grabInfo("config.ini", "Process")
    spectrogramConfig= ini().grabInfo("config.ini", "Process.Spectrogram")


    seed             = int(generalConfig['seed'])
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

    trainDataset, validationData, testDataset = setup.CreateDataset(sys.argv[1], sys.argv[2], sys.argv[3])

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
        setup.Debug(trainDataset, validationData, testDataset)

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