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