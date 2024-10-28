import tensorflow as tf

from tensorflow.keras import layers, models, Input

class ASRModel:
    def BuildModel(_shape, _size, _dataset):
            """
            This function constructs the main model, a combination of CNN and BiLSTM layers, to process spectrogram 
            data (2D time-frequency representations of audio signals).

            Parameters:
                - _shape: spectrograms (time steps, frequency bins, and channels)
                - _size: number of unique labels that the model will predict at each time step

            Returns:
                Returns a Tensorflow Model
            """
            normLayer = layers.Normalization()
            normLayer.adapt(data = _dataset.map(map_func = lambda spec, label: spec))

            inputLayer = Input(shape = _shape, name = "input")
            # resized = layers.Resizing(32, 32)(inputLayer)
            normal = normLayer(inputLayer)

            # Convolutional layer:
            #   Used for feature extraction on the spectrograms
            conv1 = layers.Conv2D(32, 3, activation='relu')(normal)
            conv2 = layers.Conv2D(64, 3, activation = 'relu')(conv1)
            pool = layers.MaxPooling2D()(conv2)

            # Reshape to fit the LSTM Layer
            reshaped = layers.Reshape((pool.shape[1], pool.shape[2] * pool.shape[3]))(pool)

            # LSTM Layer
            #   capture the temporal relationships
            lstm1 = layers.LSTM(128, return_sequences = True)(reshaped)
            lstm2 = layers.LSTM(128, return_sequences = True)(lstm1)

            dense = layers.Dense(64, activation = 'relu')(lstm2)
            outputLayer = layers.Dense(_size, activation = "linear", name = "output")(dense)

            model = models.Model(inputs = inputLayer, outputs = outputLayer)

            return model