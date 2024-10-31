import tensorflow as tf

from tensorflow.keras import layers, models, Input

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
            inputLayer = Input(shape = _shape, name = "input")

            conv1 = layers.Conv2D(32, (3, 3), activation = 'relu')(inputLayer)
            conv2 = layers.Conv2D(64, (3, 3), activation = 'relu')(conv1)
            conv3 = layers.Conv2D(128, (3, 3), activation = 'relu')(conv2)
            pool = layers.MaxPooling2D((2, 2))(conv3)

            reshaped = layers.Reshape((pool.shape[1], pool.shape[2] * pool.shape[3]))(pool)

            lstm = layers.Bidirectional(layers.LSTM(128, return_sequences = True))(reshaped)

            outputLayer = layers.TimeDistributed(layers.Dense(units = _size, activation = 'linear'))(lstm)

            model = models.Model(inputs = inputLayer, outputs = outputLayer)

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