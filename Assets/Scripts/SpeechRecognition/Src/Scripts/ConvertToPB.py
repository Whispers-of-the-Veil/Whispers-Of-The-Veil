import sys
import tensorflow as tf
from tensorflow.keras.models import load_model
from tensorflow.python.framework.convert_to_constants import convert_variables_to_constants_v2

from Training.Model.ASRModel import ASRModel

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python -m Model.ConvertToPB /path/to/model.keras /filepath/to/the/pb/model/folder")
        exit(1)
    
    print("Loading model...")
    model = load_model(
        sys.argv[1],
        custom_objects = {'ctcloss': ASRModel.ctcloss},
        safe_mode = False
    )

    print("Converting model to a frozen graph...")
    full_model = tf.function(lambda x: model(x))
    full_model = full_model.get_concrete_function(
        x = tf.TensorSpec(model.inputs[0].shape, model.inputs[0].dtype))

    frozen_func = convert_variables_to_constants_v2(full_model)
    frozen_func.graph.as_graph_def()

    print("Writing the frozen graph to the .pb file...")
    tf.io.write_graph(
        graph_or_graph_def = frozen_func.graph,
        logdir             = sys.argv[2],
        name               = "ASR.pb",
        as_text            = False
    )
    
    print(f"\nModel saved to the path {sys.argv[2]}")

    print("\nInput and Output Nodes are:")

    for input in frozen_func.inputs:
        print(input.name)

    for output in frozen_func.outputs:
        print(output.name)