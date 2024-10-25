import tensorflow as tf
import numpy as np
import h5py

from tensorflow.keras.layers import Input, Conv2D, MaxPooling2D, LSTM, Dense, Bidirectional, TimeDistributed, Reshape, Dropout
from tensorflow.keras.models import Model, load_model
from tensorflow.keras.callbacks import ReduceLROnPlateau, EarlyStopping

import sys
import os

def BuildModel(_shape, _size):
    """
    This function constructs the main model, a combination of CNN and BiLSTM layers, to process spectrogram 
    data (2D time-frequency representations of audio signals).

    Parameters:
        - _shape: spectrograms (time steps, frequency bins, and channels)
        - _size: number of unique labels that the model will predict at each time step

    Returns:
        Returns a Tensorflow Model
    """
    # Input layer of the model; uses the shape of the spectrogram
    inputLayer = Input(shape = _shape, name = "input")

    # Create 2D convolutional layers to extract feavtures from the spectrogram
    conv1 = Conv2D(filters = 32, kernel_size = (3, 3), activation = 'relu', padding = 'same')(inputLayer)
    pool1 = MaxPooling2D(pool_size = (2, 2))(conv1) # Downsamples the feature maps by a factor of 2

    conv2 = Conv2D(filters = 64, kernel_size = (3, 3), activation = 'relu', padding = 'same')(pool1)
    pool2 = MaxPooling2D(pool_size = (2, 2))(conv2) # Downsamples the feature maps by a factor of 2
    
    # Get the shape of pool2 for static reshaping
    pool_shape = pool2.shape  # (None, height, width, channels)
    
    # Reshape layer using static dimensions
    reshaped = Reshape((pool_shape[1], pool_shape[2] * pool_shape[3]))(pool2)

    # Bidirectional LSTM layers for sequential modeling
    # Read the sequence both forward and backward, which helps capture more context from the sequence.
    lstm1 = Bidirectional(LSTM(units=128, return_sequences=True))(reshaped)
    lstm1 = Dropout(0.3)(lstm1)
    lstm2 = Bidirectional(LSTM(units=128, return_sequences=True))(lstm1)
    lstm2 = Dropout(0.3)(lstm2)

    # TimeDistributed Dense layer to predict characters at each time step
    denseOutput = TimeDistributed(Dense(units = _size[0], activation = 'softmax'))(lstm2)

    model = Model(inputs = inputLayer, outputs = denseOutput)

    return model

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

def LoadH5(_file):
    with h5py.File(_file, 'r') as data:
        spectrograms = data['Spectrograms'][:]
        labels = data['Labels'][:]

    return spectrograms, labels

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

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("usage: python SpeechRecognition.py /Path/to/TrainingData/Directory /Path/to/ValidationData/Directory /output/path/to/the/model.keras")
        print("If you haven't please run the Preprocess script on the data")
        print("\nThe arguments are:")
        print(" - The path to the directory containing the batches of Training Data")
        print(" - The path to the directory containing the batches of Validation Data")
        print(" - The path to an existing model, or the path to save the model")

        exit(1)

    # Config
    seed = 42
    batchSize = 64
    numEpochs = 10

    # Set the seed value for experiment reproducibility.
    tf.random.set_seed(seed)
    np.random.seed(seed)

    print("Initializing some data...")

    with h5py.File(sys.argv[1], 'r') as data:
        inputShape = tuple(data['InputShape'][:])[1:]
        outputSize = tuple(data['OutputSize'][:])[1:]

    trainSpectrograms, trainLabels = LoadH5(sys.argv[1])
    trainDataSet = CreateDataset(trainSpectrograms, trainLabels, batchSize)
    del trainSpectrograms, trainLabels
    
    validSpectrograms, validLabels = LoadH5(sys.argv[2])
    validDataSet = CreateDataset(validSpectrograms, validLabels, batchSize)
    del validSpectrograms, validLabels

    # Load existing model if it exists
    if os.path.exists(sys.argv[3]):
        print(f"Loaded existing model {sys.argv[3]}")

        model = load_model(sys.argv[3], custom_objects = {'ctcLoss': ctcLoss}, safe_mode = False)
    else:
        print(f"Building a new model")

        model = BuildModel(inputShape, outputSize)
        model.compile(optimizer = 'adam', loss = ctcLoss)

    reduce_lr = ReduceLROnPlateau(monitor = 'val_loss', factor = 0.5, patience = 3, min_lr = 1e-6)
    early_stopping = EarlyStopping(monitor = 'val_loss', patience = 5, restore_best_weights = True)

    model.fit(
        trainDataSet,
        validation_data = validDataSet,
        epochs          = numEpochs,
        verbose         = 1,
        callbacks       = [reduce_lr, early_stopping]
    )

    model.save(sys.argv[3])
    model.summary()