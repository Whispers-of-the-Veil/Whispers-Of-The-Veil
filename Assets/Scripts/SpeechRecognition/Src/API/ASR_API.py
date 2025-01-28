
import sys
import os
import numpy as np
from flask import Flask, jsonify, request
from tensorflow.keras.models import load_model
from io import BytesIO

from Data.Process import Process
from Model.ASRModel import ASRModel
from Grab_Ini import ini

app = Flask(__name__)

class Recognition:
    def __init__(self):
        if getattr(sys, 'frozen', False):
            scriptDir = sys._MEIPASS 
        else:
            scriptDir = os.path.dirname(os.path.abspath(__file__))

        filePath = os.path.join(scriptDir, 'models', 'ASR.keras')

        self.model = load_model(filePath, custom_objects = {'ctcloss': ASRModel.ctcloss}, safe_mode = False)
        self.process = Process()

    def Predict(self, _audio):
        spectrogram = np.expand_dims(self.process._Spectrogram(_audio), axis = 0)

        sentiment = self.model.predict(spectrogram)
        softmax = ASRModel.ctcDecoder(sentiment)
        prediction = self.process.ConvertLabel(softmax)

        return prediction
        
@app.route('/ASR', methods = ['POST'])
def api_service():
    try:
        print("\nGenerating Prediction...")
        audio_bytes = request.data

        audio_data = np.frombuffer(audio_bytes, dtype=np.float32)
        prediction = recognition.Predict(audio_data)
        
        return jsonify({"prediction": prediction}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500

def main():
    try:
        print("\nLoading model...")
        global recognition
        recognition = Recognition()

        print("\nStarting Automatic-Speech-Recognition API...")
        app.run(host='0.0.0.0', port = 8888)
    except Exception as e:
        print(f"Error starting the server: {str(e)}")

if __name__ == "__main__":
    main()