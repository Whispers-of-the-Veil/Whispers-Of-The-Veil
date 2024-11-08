import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
from tensorflow.keras import backend

class ASRModel:   
    def BuildModel(_shape, _numClasses):
            """
            Disclaimer:
                This model was derived from the deepspeech models documentation found at:
                https://deepspeech.readthedocs.io/en/v0.6.1/DeepSpeech.html
                https://github.com/mozilla/DeepSpeech/blob/master/LICENSE

                Credit to this architecture design goes to the mozilla team.

            This model is comprised of seven layers (excluding the input layer). The first layer is a
            one dimensional convolutional nerual network, which is used to extract features from the MFCCs.
            The following three layers are are fully connected Dense layers with ReLU activation. These layers
            take the feautures extracted from the convolutional layer and determine any patterns or relationships.
            The Recurrent Logn Short-term Memory layer helps the model correctly predict sequence of characters or words.
            The last Dense layer with the ReLU activation helps to tie together the results of the previous layers.

            Each of these layers are followed by a BatchNormalization layer to help stabilize the outputs of each
            layer before passing it to the next.

            Parameters:
                - _shape: This is the shape of the MFCC features
                - _numClasses: This is the number of output classes that the predictions should have.
                               This number should reflect the number of entries in the charToIndiceMap

            Returns:
                A Keras sequential model following the deepspeech model architecture
            """
            model = keras.Sequential();

            model.add(layers.Input(shape = _shape))

            model.add(layers.Conv2D(filters = 32, kernel_size = [11, 41], strides = [2, 2], padding = 'same', use_bias=False))
            model.add(layers.BatchNormalization())
            model.add(layers.ReLU())
            model.add(layers.Conv2D(filters = 32, kernel_size = [11, 21], strides = [1, 2], padding = 'same', use_bias=False))
            model.add(layers.BatchNormalization())
            model.add(layers.ReLU())
            model.add(layers.MaxPooling2D(pool_size=(2, 2)))

            # # Adding another MaxPooling2D layer to further reduce the time steps 
            # model.add(layers.Conv2D(filters=64, kernel_size=(11, 21), strides=(1, 1), padding='same')) 
            # model.add(layers.BatchNormalization())
            # model.add(layers.MaxPooling2D(pool_size=(2, 2)))

            model.add(layers.Reshape((-1, 64)))
            # model.add(layers.Reshape((-1, model.output_shape[-1] * model.output_shape[-2])))

            for _ in range(5):
                model.add(layers.Bidirectional(layers.GRU(
                    units = 800, 
                    activation="tanh",
                    recurrent_activation="sigmoid",
                    use_bias=True,
                    return_sequences=True,
                    reset_after=True
                    ), merge_mode="concat"
                ))
                model.add(layers.Dropout(0.5))

            # , activation='relu', dropout = 0.5
            model.add(layers.Dense(units = 1600, activation='relu'))
            model.add(layers.Dropout(0.5))
            
            model.add(layers.TimeDistributed(layers.Dense(units = _numClasses + 1, activation = "softmax")))

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

    def CTCLoss(y_true, y_pred):
        # Compute the training-time loss value
        batch_len = tf.cast(tf.shape(y_true)[0], dtype="int64")
        input_length = tf.cast(tf.shape(y_pred)[1], dtype="int64")
        label_length = tf.cast(tf.shape(y_true)[1], dtype="int64")

        input_length = input_length * tf.ones(shape=(batch_len, 1), dtype="int64")
        label_length = label_length * tf.ones(shape=(batch_len, 1), dtype="int64")

        print(tf.shape(y_true)[0])
        print(tf.shape(y_pred)[1])
        print(tf.shape(y_true)[1])

        loss = keras.backend.ctc_batch_cost(y_true, y_pred, input_length, label_length)
        return loss


    # Custom CTC Greedy Decoder for evaluation
    def ctc_greedy_decoder(y_pred):
        input_length = tf.fill([tf.shape(y_pred)[0]], tf.shape(y_pred)[1])
        decoded, _ = tf.nn.ctc_greedy_decoder(y_pred, input_length)
        return decoded