import tensorflow as tf
import numpy as np

from tensorflow.keras.layers import Input, Conv2D, MaxPooling2D, LSTM, Dense, Bidirectional, TimeDistributed, Lambda
from tensorflow.keras.models import Model, load_model

from tensorflow.keras.preprocessing.sequence import pad_sequences

import sys
import os

class PrepareData:
    def __init__(self): pass

    def PrepareLabels(self, _transcripts, _charIndex, _maxLength):
        """
        Takes in a list of transcripts, converts them into their corresponding character indices 
        using a provided character index dictionary, and pads them to ensure they all have the same length

        Parameters:
            - _transcript: A list of transcripts
            - _charIndex: A dictionary mapping characters to their respective indices
            - _maxLength: The maximum length of the sequences after padding

        Returns:
            Returns a list of processed transcripts
        """
        indexedTranscripts = self.TranscriptToIndices(_transcripts, _charIndex)  # Convert to indices

        # Ensure that the input is a list of lists
        if not all(isinstance(i, list) for i in indexedTranscripts):
            raise ValueError("Indexed transcripts should be a list of lists.")

        padded_labels = pad_sequences(indexedTranscripts, maxlen = _maxLength, padding = 'post', value = _charIndex['<PAD>'])  # Use index 0 for padding

        return padded_labels

    def TranscriptToIndices(self, _transcripts, _charIndex):
        """
        Converts the text transcripts into a list of integer indices using a provided a dictionary of chracter mappings

        Parameters:
            - _trascripts: A list of transcripts
            - _charIndex: A dictionary mapping characters to their respective indices
        
        Returns:
            A lists containing the integer indices for each transcript
        """
        indexedTranscripts = []

        for transcript in _transcripts:
            # Convert each character in the transcript to its corresponding index
            indexedTranscript = [_charIndex[char] for char in transcript if char in _charIndex]  # Ensure each char exists in the mapping
            indexedTranscripts.append(indexedTranscript)  # Append the list of indices to the main list

        return indexedTranscripts

    def CreateVocabulary(self, _transcripts):
        """
        Create a dictionary mapping of characters to integer from a list of transcripts.

        Parameters:
            - _trainscripts: A list of transcripts
        
        Returns:
            returns a chracter-to-indice dictionary
        """
        uniqueChar = sorted(set(''.join(_transcripts)))  # Unique characters in all transcripts
        charIndex = {char: index + 1 for index, char in enumerate(uniqueChar)}  # Indexing starting from 1
        charIndex['<PAD>'] = 0  # Adding a padding character

        return charIndex

    def CreateDataset(self, _audioData, _transcriptData, _batchSize):
        """
        Creates a TensorFlow dataset from pairs of audio data and transcript data, 
        shuffles the data, batches it, and optimizes it for processing.

        Parameters:
            - _audioData: An array of audio data samples
            - _trainscriptData: An array of transcript data for the audio samples
            - _batchSize: Defines the size of the batches that the dataset will be divided by

        Returns:
            Returns a Tensorflow dataset
        """
        dataSet = tf.data.Dataset.from_tensor_slices((_audioData, _transcriptData))
        dataSet = dataSet.shuffle(buffer_size = len(_audioData))
        dataSet = dataSet.batch(_batchSize)
        dataSet = dataSet.prefetch(tf.data.AUTOTUNE)

        return dataSet

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

    # Create 2D convolutional layers to extract features from the spectrogram
    conv1 = Conv2D(filters = 32, kernel_size = (3, 3), activation = 'relu', padding = 'same')(inputLayer)
    pool1 = MaxPooling2D(pool_size = (2, 2))(conv1) # Downsamples the feature maps by a factor of 2

    conv2 = Conv2D(filters = 64, kernel_size = (3, 3), activation = 'relu', padding = 'same')(pool1)
    pool2 = MaxPooling2D(pool_size = (2, 2))(conv2) # Downsamples the feature maps by a factor of 2

    # Use a Lambda layer to dynamically compute the reshape dimensions
    # Need so it can be processed by the LSTM layers
    reshaped = Lambda(lambda x: tf.reshape(x, (-1, tf.shape(x)[1], tf.shape(x)[2] * tf.shape(x)[3])))(pool2)

    # Bidirectional LSTM layers for sequential modeling
    # Read the sequence both forward and backward, which helps capture more context from the sequence.
    lstm1 = Bidirectional(LSTM(units=128, return_sequences = True))(reshaped)
    lstm2 = Bidirectional(LSTM(units=128, return_sequences = True))(lstm1)

    # TimeDistributed Dense layer to predict characters at each time step
    denseOutput = TimeDistributed(Dense(units = _size, activation = 'softmax'))(lstm2)

    model = Model(inputs = inputLayer, outputs = denseOutput)

    return model

def ctcLoss(_yTrue, _yPred):
    """
    CTC Loss using TensorFlow's `tf.nn.ctc_loss`.

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
        print("usage: python SpeechRecognition.py traininginput.npz devinput.npz /output/path/to/the/model.keras")
        print("If you haven't please run the Preprocess script on the data")
        print("\nThe arguments are:")
        print(" - The Processed Training Data")
        print(" - The Processed Validation Data")
        print(" - The path to the model")
        print("     - If the path is to an existing model, the program will use that to train instead")
        print("     - Otherwise if the path doesn't exist, it will create a new model")

        exit(1)
    
    prepare = PrepareData()

    # Set the seed value for experiment reproducibility.
    seed = 42
    tf.random.set_seed(seed)
    np.random.seed(seed)
    batchSize = 8

    print("Loading processed training and validiation data")
    trainingData = np.load(sys.argv[1]) # Load in the preprocessed training data
    devData = np.load(sys.argv[2])      # Load in the preprocessed dev data
    
    # Extract the data from the npz files
    trainAudioData = trainingData['audioData']
    trainTranscript = trainingData['transcriptData']

    devAudioData = devData['audioData']
    devTranscripts = devData['transcriptData']

    # Expand the dimensions for channels
    trainAudioData = np.expand_dims(trainAudioData, axis=-1)  # Shape: (batch_size, height, width, 1)
    devAudioData = np.expand_dims(devAudioData, axis=-1)      # Shape: (batch_size, height, width, 1)

    # Create vocabulary and prepare labels
    charToIndex = prepare.CreateVocabulary(trainTranscript)
    maxLength = max(len(t) for t in trainTranscript)  # Calculate max length of transcripts for padding

    trainLabels = prepare.PrepareLabels(trainTranscript, charToIndex, maxLength)
    devLabels = prepare.PrepareLabels(devTranscripts, charToIndex, maxLength)

    # Create Data Sets from those npz files
    print("Creating Tensorflow datasets from the processed data")
    trainDataSet = prepare.CreateDataset(trainAudioData, trainLabels, batchSize)
    devDataSet = prepare.CreateDataset(devAudioData, devLabels, batchSize)

    # Define model parameters
    inputShape = trainAudioData.shape[1:]                   # shape of the spectrogram (timesteps, frequency bins, channels)
    outputSize = len(set(''.join(trainTranscript))) + 1     # Size of the output layer (number of unique characters)

    # Load existing model if it exists
    if os.path.exists(sys.argv[3]):
        print(f"Loaded existing model {sys.argv[3]}")

        model = load_model(sys.argv[3], custom_objects = {'ctcLoss': ctcLoss})
    else:
        print(f"Building a new model")

        model = BuildModel(inputShape, outputSize)
        model.compile(optimizer = 'adam', loss = ctcLoss)
    
    # Train the model
    model.fit(trainDataSet, validation_data = devDataSet, epochs = 10, verbose = 1)

    model.save(sys.argv[3])                         # Save the model to the defined path
    model.summary()                                 # Display a summary of the models archtecture