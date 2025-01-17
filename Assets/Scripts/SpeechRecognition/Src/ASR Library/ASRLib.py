import ctypes
import tensorflow as tf
import numpy as np

from Data.Preporcess import Process
from Model.ASRModel import ASRModel

@ctypes.CFUNCTYPE(ctypes.POINTER(ctypes.c_char), ctypes.POINTER(ctypes.c_float), ctypes.c_int, ctypes.POINTER(ctypes.c_char), ctypes.c_int)
def Predict(_audioptr, _length, _modelptr, _modelSize):
    """
    This is a wrapper function that will be used to interact with the model outside
    of python.

    Parameters:
        - audio_ptr: A pointer to an audio sample held in memory
        - audio_length: The length of the audio sample
        - model_data_ptr: A pointer to the model data in memory
        - model_size: The size of the model data in bytes

    Returns:
        The decoded prediction from the model
    """
    # We dont need to define an instance for the ASRModel class. It will throw
    # an error if we try to access any of the registered methods if we do. 
    process = Process()

    audio = np.ctypeslib.as_array(_audioptr, shape = (_length,))
    audio = tf.convert_to_tensor(audio, dtype = tf.float32)

    # The input node (x:0) requires the shape (none, none, 193) and the frozen graph
    # cannot use a tf.Tensor Object. Thus, we have to convert the results from the process._Spectrogram
    # call to a ndarray and expand the first diemension to 1 
    spectrogram = process._Spectrogram(audio)
    spectrogram = np.expand_dims(spectrogram.numpy(), axis = 0)

    modelData = ctypes.string_at(_modelptr, _modelSize)
    graphDef = tf.compat.v1.GraphDef()
    graphDef.ParseFromString(modelData)

    with tf.compat.v1.Graph().as_default() as graph: 
        tf.import_graph_def(graphDef, name = "")

    with tf.compat.v1.Session(graph = graph) as sess:
        inputNode = graph.get_tensor_by_name("x:0")
        outputNode = graph.get_tensor_by_name("Identity:0")

        logits = sess.run(outputNode, feed_dict = {inputNode: spectrogram})

    sentiment = ASRModel.ctcDecoder(logits)
    prediction = process.ConvertLabel(sentiment)

    return prediction.encode('utf-8')