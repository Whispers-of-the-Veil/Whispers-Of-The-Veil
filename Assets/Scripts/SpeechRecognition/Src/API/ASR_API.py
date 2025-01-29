import numpy as np
from flask import Flask, jsonify, request
from io import BytesIO

from Recognition import SpeechRec

from Grab_Ini import ini

app = Flask(__name__)
        
@app.route('/ASR', methods = ['POST'])
def api_service():
    try:
        print("\nGenerating Prediction...")
        audio_bytes = request.data

        audio_data = np.frombuffer(audio_bytes, dtype=np.float32)
        prediction = speechRec.Predict(audio_data)
        
        return jsonify({"prediction": prediction}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500

def main():
    try:
        print("Loading Config...")
        apiConfig = ini().grabInfo("config.ini", "API")
        port = int(apiConfig['port'])

        print("\nLoading model...")
        global speechRec
        speechRec = SpeechRec()

        print("\nStarting API...")
        app.run(host = '0.0.0.0', port = port)
    except Exception as e:
        print(f"Error starting the server: {str(e)}")

if __name__ == "__main__":
    main()