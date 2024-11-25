import tensorflow as tf
import numpy as np
import matplotlib.pyplot as plt

from tensorflow.keras.models import load_model
from tensorflow.keras.optimizers import Adam

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
    dataSet = tf.data.Dataset.from_tensor_slices((_spectrograms, _labels))
    dataSet = dataSet.shuffle(buffer_size = len(_spectrograms))
    dataSet = dataSet.batch(_batchSize)
    dataSet = dataSet.prefetch(tf.data.AUTOTUNE)

    return dataSet

def DataGenerator(_audioPath, _transcripts, _iterAmount, _batchSize, _validSet):
    """
    This function will generate and yied batches of data from given audio paths and transcripts.

    Parameters:
        - _audioPath: A list containing the complete file paths of the audio files to process
        - _transcripts: A list containing the transcripts of those audio files
        - _iterAmount: The iteration amount defining how many iterations we need to move through before reseting
        - _batchSize: The size of the batches that the generator will yield
        - _validSet: A boolean used in the Proess class to determine if we are processing a validation set
                     (The config file as a seperate smaller value for the validation set)

    Yields:
        A batch from a tensorflow dataset
    """
    epoch = 0
    
    while True:
        # We are taking the modulo of the current epoch and the iteration amount to
        # ensure that we reset the index when we reach multiples of that amount.
        # This will allow us to continuously generate data throughout the defined number
        # of epochs. While staying within the bounds of the audio path.
        index = epoch % _iterAmount

        # The process method will only process up to the defined sample size in the config.ini
        # file. This is done to help conserve resources, since we are holding the spectrograms in
        # memory.
        # For that same reason, we are processing the data on the fly instead of saving them to the disk.
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
    
    tf.random.set_seed(seed)
    np.random.seed(seed)

    trainAudioPaths, trainTranscripts = PROCESS.LoadCSV(sys.argv[1])
    validAudioPaths, validTranscripts = PROCESS.LoadCSV(sys.argv[2])

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

    # The Iteration number are used so the Process class doesn't yield None when attempting to
    # access data outside the bounds of the audioPaths. We are taking the ceiling of that result
    # so we can account for the small chunk of data that doesnt fit in the sample size defined in the
    # config.ini file.
    # For example, if we have 32000 audio files to process, with 5000 samples per iteration, we would have
    # 7 iterations to move through to process the entire dataset; 6 iterations of the full 5000 samples, and
    # one with only 2000.
    trainIter = math.ceil(len(trainAudioPaths) / samples)
    validIter = math.ceil(len(validAudioPaths) / valSamples)

    trainGen = DataGenerator(trainAudioPaths, trainTranscripts, trainIter, batchSize, False)
    validGen = DataGenerator(validAudioPaths, validTranscripts, validIter, batchSize, True)

    # The steps per epoch (both the validation and training) is set to the number of samples divided by
    # the batch size, this is done so that it will train off the entire generated set in one epoch before
    # generating the next set.
    history = model.fit(
        trainGen,
        steps_per_epoch  = samples // batchSize,
        epochs           = numEpochs,
        verbose          = 1,
        validation_data  = validGen,
        validation_steps = valSamples // batchSize
    )

    PlotLoss(history)

    model.save(sys.argv[3])
    model.summary()