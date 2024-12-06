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

        model.add(layers.Dense(units = _numClasses + 1))

        return model

    @register_keras_serializable(name = "ctcloss")
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

    @register_keras_serializable(name = "ctcDecoder")
    def ctcDecoder(_logits):
        """
        

        Parameters:
            - _logits:

        Returns:
            A list of the decoded sequence
        """
        decoded = tf.keras.ops.ctc_decode (
            _logits,
            np.ones(_logits.shape[0]) * _logits.shape[1],
            strategy = 'greedy'
        )[0][0]

        return decoded