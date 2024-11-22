import tensorflow as tf
import numpy as np
import matplotlib.pyplot as plt

from tensorflow.keras.models import load_model
from tensorflow.keras.callbacks import ReduceLROnPlateau, EarlyStopping
from tensorflow.keras.optimizers import Adam

from tqdm import tqdm
import sys
import os
import math

from Model.ASRModel import ASRModel
from Grab_Ini import ini
from Data.Preporcess import Process

PROCESS = Process()

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

def DataGenerator(_audioPath, _transcripts, _iterAmount, _batchSize, _validSet):
    epoch = 0
    
    while True:
        index = epoch % _iterAmount

        spectrograms = PROCESS.Audio(_audioPath, index, _validSet)
        labels       = PROCESS.Transcript(_transcripts, index, _validSet)

        for batch in CreateDataset(spectrograms, labels, _batchSize):
            yield batch
        
        epoch += 1

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("usage: python TrainASRModel.py /Path/to/TrainingData.csv /Path/to/ValidationData.csv /output/path/to/the/model.keras")
        exit(1)

    print("Loading Config")
    generalConfig       = ini().grabInfo("config.ini", "General")
    trainingConfig      = ini().grabInfo("config.ini", "Training")
    learningConfig      = ini().grabInfo("config.ini", "Training.LearningRate")
    preprocessingConfig = ini().grabInfo("config.ini", "Preprocess")

    seed            = int(generalConfig['seed'])

    batchSize       = int(trainingConfig['batch_size'])
    numEpochs       = int(trainingConfig['epochs'])

    lr              = float(learningConfig['learning_rate'])
    decaySteps      = int(learningConfig['decay_steps'])
    decayRate       = float(learningConfig['decay_rate'])

    samples         = int(preprocessingConfig['samples'])
    valSamples      = int(preprocessingConfig['validation_samples'])

    # Set the seed value for experiment reproducibility.
    tf.random.set_seed(seed)
    np.random.seed(seed)

    trainAudioPaths, trainTranscripts = PROCESS.LoadCSV(sys.argv[1])
    validAudioPaths, validTranscripts = PROCESS.LoadCSV(sys.argv[2])

    trainIter = math.ceil(len(trainAudioPaths) / samples)
    validIter = math.ceil(len(validAudioPaths) / valSamples)

    print(f"Training with {len(trainAudioPaths)} samples")
    print(f"Validating with {len(validAudioPaths)} sampels")

    expDecayLR = tf.keras.optimizers.schedules.ExponentialDecay(
        lr,
        decay_steps = decaySteps,
        decay_rate = decayRate,
        staircase = True)

    if os.path.exists(sys.argv[3]):
        print(f"\nLoaded existing model {sys.argv[3]}")

        model = load_model(
            sys.argv[3], 
            custom_objects = {'ctcLoss': ASRModel.ctcloss}, 
            safe_mode = False
        )
    else:
        print(f"\nBuilding a new model")

        model = ASRModel.BuildModel((96, 563, 1), 28)

        model.compile(
            optimizer   = Adam(learning_rate = expDecayLR), 
            loss = ASRModel.ctcloss
        )

    model.summary()

    trainGen = DataGenerator(trainAudioPaths, trainTranscripts, trainIter, batchSize, False)
    validGen = DataGenerator(validAudioPaths, validTranscripts, validIter, batchSize, True)

    history = model.fit(
        trainGen,
        steps_per_epoch  = trainIter,
        validation_data  = validGen,
        validation_steps = validIter,
        epochs           = numEpochs
    )

    PlotLoss(history)

    model.save(sys.argv[3])
    model.summary()