import tensorflow as tf
import numpy as np
from tensorflow.keras.models import Model, load_model

import sys

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

def TestSentiment(_model, _spectrogram, _transcript):
    try:
        # Predict the sentiment class for the spectrogram
        sentiment = _model.predict(np.expand_dims(_spectrogram, axis=0))  # Add batch dimension for prediction
        predicted_classes = np.argmax(sentiment, axis=-1)  # Assuming model output is a probability vector

        # Flatten the predicted sequence (assuming 1D transcript)
        predicted_classes = predicted_classes.flatten()

        # Compare predicted class with transcript (assuming transcript is class label)
        assert predicted_classes[0] == _transcript, f"Prediction {predicted_classes} does not match transcript {_transcript}"

    except Exception as e:
        print(f"An error occurred during testing: {e}")
if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python TestModel /path/to/model.keras /path/to/testData.npz")
        exit(1)

    print("Loading Test dataset")
    testData = np.load(sys.argv[2])

    testSpectrogam = testData['Spectrograms']
    testTranscript = testData['Transcript']
    
    model = load_model(sys.argv[1],  custom_objects={'ctcLoss': ctcLoss}, safe_mode = False)

    print("Asserting models preformance")
    for sample, transcript in zip(testSpectrogam, testTranscript):
        TestSentiment(model, sample, transcript)