import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
from tensorflow.keras.backend import ctc_batch_cost

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
            model = keras.Sequential();

            model.add(layers.Input(shape = _shape, name = "input"))

            model.add(layers.Bidirectional(layers.LSTM(128, return_sequences = True, activation = 'tanh')))
            model.add(layers.Bidirectional(layers.LSTM(64, return_sequences = True, activation = 'tanh')))

            model.add(layers.Dense(64, activation = 'relu'))

            model.add(layers.TimeDistributed(layers.Dense(_size, activation = 'softmax')))

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
        _yPred = tf.reshape(_yPred, [-1, 128, 28])
        batchSize = tf.shape(_yPred)[0]
        timeSteps = tf.shape(_yPred)[1]

        inputLength = tf.fill([batchSize], timeSteps)
        labelLength = tf.reduce_sum(tf.cast(_yTrue != 0, tf.int32), axis=-1)

        _yTrue = tf.cast(_yTrue, tf.int32)

        loss = tf.nn.ctc_loss(
            labels              =   _yTrue,
            logits              =   _yPred,
            label_length        =   labelLength,
            logit_length        =   inputLength,
            logits_time_major   =   False,
            blank_index         =   -1
        )

        return tf.reduce_mean(loss)