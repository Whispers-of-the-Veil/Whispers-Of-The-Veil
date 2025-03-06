import os
import sys
import numpy as np
from flask import Flask, jsonify, request
from multiprocessing import Process
from io import BytesIO

from Recognition import SpeechRec
from Grab_Ini import ini

app = Flask(__name__)
close = False

@app.route('/ASR', methods = ['POST'])
def api_service():    
    try:
        print("\nGenerating Prediction...")
        audio_bytes = request.data

        audio_data = np.frombuffer(audio_bytes, dtype = np.float32)

        prediction = speechRec.Predict(audio_data)
        prediction = speechRec.PostProcess(prediction)
        
        return jsonify({"prediction": prediction}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500
    
@app.route('/close', methods = ['GET'])
def close_api():
    os._exit(0)
    
def main():
    global speechRec

    try:
        print("Loading Config...")
        apiConfig = ini().grabInfo(os.path.join(os.path.dirname(sys.executable), "config.ini"), "API")
        port = int(apiConfig['port'])

        print("\nLoading model...")
        speechRec = SpeechRec()

        print("\nStarting API...")
        app.run(host = '0.0.0.0', port = port)
    except Exception as e:
        print(f"Error starting the server: {str(e)}")

if __name__ == "__main__":
    main()