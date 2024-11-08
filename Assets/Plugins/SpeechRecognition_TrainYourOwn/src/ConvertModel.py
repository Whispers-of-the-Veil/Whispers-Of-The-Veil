import tensorflow as tf
import sys
from ASRModel import ASRModel as asrmodel

# Load the Keras model
model = tf.keras.models.load_model(
    sys.argv[1],
    custom_objects = {'ctcLoss': asrmodel.ctcLoss}, 
    safe_mode = False
)

# Convert the model to TFLite format
converter = tf.lite.TFLiteConverter.from_keras_model(model)
converter.target_spec.supported_ops = [
  tf.lite.OpsSet.TFLITE_BUILTINS, # enable LiteRT ops.
  tf.lite.OpsSet.SELECT_TF_OPS # enable TensorFlow ops.
]

tflite_model = converter.convert()

# Save the converted TFLite model
with open(f'{sys.argv[2]}.tflite', 'wb') as f:
    f.write(tflite_model)