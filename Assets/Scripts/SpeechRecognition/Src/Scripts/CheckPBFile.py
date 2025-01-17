
import tensorflow as tf
import numpy as np
import sys

from Data.Preporcess import Process
from Model.ASRModel import ASRModel

def LoadModel(_filePath):
    """
    This function will load the frozen graph from the provided .pb file.

    Parameters:
        - _filePath: the file path to the .pb file
    
    Returns:
        A graph object
    """
    with tf.io.gfile.GFile(_filePath, "rb") as f:
        graphDef = tf.compat.v1.GraphDef()
        graphDef.ParseFromString(f.read())

    with tf.compat.v1.Graph().as_default() as graph:
        tf.import_graph_def(graphDef, name = "")

    return graph

def Predict(_graph, _inputData):
    """
    This function runs a forward pass on the _inputData using the frozen graph.
    
    Parameters:s
        - _graph: A graph object
        - _inputData: ndarray object of the input data

    Returns:
        The prediction
    """
    with tf.compat.v1.Session(graph = _graph) as sess:
        inputNode = _graph.get_tensor_by_name("x:0")
        outputNode = _graph.get_tensor_by_name("Identity:0")

        prediction = sess.run(outputNode, feed_dict = {inputNode: _inputData})
    return prediction

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python -m Model.CheckPBFile /path/to/model.pb /path/to/data.csv")

    process = Process()

    audioPaths, transcripts = process.LoadCSV(sys.argv[2])

    print("Loading the frozen graph...")
    graph = LoadModel(sys.argv[1])

    print("Starting to run predictions")
    for i in range(len(audioPaths)):
        testSpectrogram, testLabel = process.Data(audioPaths[i], transcripts[i])

        # The input node (x:0) requires the shape (none, none, 193) and the frozen graph
        # cannot use a tf.Tensor Object. Thus, we have to convert the results from the process.Data
        # call to a ndarray and expand the first diemension to 1 
        sentiment = Predict(graph, np.expand_dims(testSpectrogram.numpy(), axis = 0))
        prediction = ASRModel.ctcDecoder(sentiment)
        prediction = process.ConvertLabel(prediction)

        print(prediction)

        check = input("Continue? [Enter or q] ")

        if check == 'q':
            break