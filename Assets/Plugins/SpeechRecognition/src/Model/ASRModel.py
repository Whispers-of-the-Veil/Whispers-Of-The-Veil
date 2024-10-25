import tensorflow as tf

from tensorflow.keras.layers import Input, Conv2D, MaxPooling2D, LSTM, Dense, Bidirectional, TimeDistributed, Reshape, Dropout
from tensorflow.keras.models import Model


class ASRModel:
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
        # Input layer
        inputLayer = Input(shape = _shape, name = "input")

        # Convolutional layer:
        #   Used for feature extraction on the spectrograms
        conv1 = Conv2D(32, 3, activation = 'relu')(inputLayer)
        conv2 = Conv2D(64, 3, activation = 'relu')(conv1)
        pool = MaxPooling2D(2)(conv2)

        # Reshape to fit the LSTM Layer
        reshaped = Reshape((-1, pool.shape[-1] * pool.shape[-2]))(pool)

        # LSTM Layer
        #   capture the temporal relationships
        lstm1 = LSTM(128, return_sequences = True)(reshaped)
        lstm2 = LSTM(128, return_sequences = True)(lstm1)
        droupt = Dropout(0.3)(lstm2)

        # TimeDistributed Layer (Output layer)
        #   for sequence prediction
        outputLayer = TimeDistributed(Dense(_size[0], activation = 'softmax'), name = "output")(droupt)

        model = Model(inputs = inputLayer, outputs = outputLayer)

        return model