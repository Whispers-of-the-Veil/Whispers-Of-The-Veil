import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers

from keras.saving import register_keras_serializable
from tensorflow.keras.regularizers import l2
import numpy as np

class ASRModel():
    def BuildModel(_shape, _numClasses):
        """
        Disclaimer:
            This Model's architecture was derived from the deepspeech 2 documentation & its github page found at:
                https://nvidia.github.io/OpenSeq2Seq/html/speech-recognition/deepspeech2.html
                https://github.com/NVIDIA/OpenSeq2Seq/blob/master/example_configs/speech2text/ds2_small_1gpu.py

            Included in this same directory is a physical copy of the LISCENSE found on the Github page,
            bellow is the link to it as well:
                https://github.com/NVIDIA/OpenSeq2Seq/blob/master/LICENSE

        The described architecture was converted to a keras sequential model

        Parameters:
            - _shape: This is the shape of the MFCC features
            - _numClasses: This is the number of output classes that the predictions should have.

        Returns:
            A Keras sequential model following the deepspeech 2 model architecture
        """
        model = keras.Sequential()

        model.add(layers.Input((None, _shape)))
        model.add(layers.Reshape((-1, _shape, 1)))            

        model.add(layers.Conv2D(
            filters = 32, 
            kernel_size = [11, 41], 
            strides = [2, 2], 
            padding = 'same',
            use_bias = False,
            kernel_regularizer = l2(0.0005)
        ))
        model.add(layers.BatchNormalization())
        model.add(layers.ReLU())
        
        model.add(layers.Conv2D(
            filters = 32, 
            kernel_size = [11, 41], 
            strides = [2, 2], 
            padding = 'same', 
            use_bias = False,
            kernel_regularizer = l2(0.0005)
        ))
        model.add(layers.BatchNormalization())
        model.add(layers.ReLU())

        model.add(layers.Reshape((-1, model.output_shape[-2] * model.output_shape[-1])))

        for i in range(1, 5 + 1):
            model.add(layers.Bidirectional(layers.GRU(units = 512, return_sequences = True)))

            if i < 5:                     
                model.add(layers.Dropout(0.5))

        model.add(layers.Dense(units = 1024))
        model.add(layers.ReLU())
        model.add(layers.Dropout(0.5))

        model.add(layers.Dense(units = _numClasses + 1, activation = "softmax"))

        return model

    @register_keras_serializable()
    def ctcloss(_yTrue, _yPred):
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
        batchSize = tf.cast(tf.shape(_yTrue)[0], dtype = "int64")
        inputLength = tf.cast(tf.shape(_yPred)[1], dtype = "int64")
        labelLength = tf.cast(tf.shape(_yTrue)[1], dtype = "int64")

        inputLength = inputLength * tf.ones(shape = (batchSize, 1), dtype = "int64")
        labelLength = labelLength * tf.ones(shape = (batchSize, 1), dtype = "int64")

        loss = keras.backend.ctc_batch_cost(_yTrue, _yPred, inputLength, labelLength)

        return tf.reduce_mean(loss)

    @register_keras_serializable()
    def ctcDecoder(_pred):
        """
        Decodes the output of the model. Expects the output to be a softmax predictions.

        Parameters:
            - _pred: A tensor of the predictions 

        Returns:
            A list of the decoded sequence
        """
        decoded = keras.backend.ctc_decode(
                _pred,
                np.ones(_pred.shape[0]) * _pred.shape[1],
                greedy = True
        )[0][0]

        return decoded