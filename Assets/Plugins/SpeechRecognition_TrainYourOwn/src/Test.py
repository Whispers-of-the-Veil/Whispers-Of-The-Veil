import tensorflow as tf

# Load the TFLite model and allocate tensors.
interpreter = tf.lite.Interpreter(model_path="/home/ldavis/Code/Data_Sets/SpeechRec/Model/deepspeech-0.9.3-models.tflite")
interpreter.allocate_tensors()

# Get input details
input_details = interpreter.get_input_details()
print("Input details:")
for input_tensor in input_details:
    print(input_tensor)

# Get output details
output_details = interpreter.get_output_details()
print("\nOutput details:")
for output_tensor in output_details:
    print(output_tensor)