import tensorflow as tf

from tensorflow.keras.layers import Input, Conv2D, MaxPooling2D, LSTM, Dense, Bidirectional, TimeDistributed, Reshape, Dropout, BatchNormalization
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
            conv1 = Conv2D(16, (3, 3), activation = 'relu', padding = 'same')(inputLayer)
            bn1 = BatchNormalization()(conv1)
            conv2 = Conv2D(32, (3, 3), activation = 'relu', padding = 'same')(bn1)
            bn2 = BatchNormalization()(conv2)
            pool = MaxPooling2D((2,2))(bn2)

            # Reshape to fit the LSTM Layer
            reshaped = Reshape((pool.shape[1], pool.shape[2] * pool.shape[3]))(pool)

            # LSTM Layer
            #   capture the temporal relationships
            lstm1 = LSTM(64, return_sequences = True)(reshaped)
            lstm2 = LSTM(64, return_sequences = True)(lstm1)
            droupt = Dropout(0.3)(lstm2)

            # TimeDistributed Layer (Output layer)
            #   for sequence prediction
            outputLayer = TimeDistributed(Dense(_size), name = "output")(droupt)

            model = Model(inputs = inputLayer, outputs = outputLayer)

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
            label_length        =   labelLength,
            logit_length        =   inputLength,
            logits_time_major   =   False,
            blank_index         =   -1              # Last class used as the blank index
        )

        return tf.reduce_mean(loss)