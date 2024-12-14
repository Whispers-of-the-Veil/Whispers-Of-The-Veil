# Lucas Davis

import sys
import pyaudio
import numpy as np
import tensorflow as tf
from tensorflow.keras.models import load_model

from Model.ASRModel import ASRModel
from Data.Preporcess import Process

CHUNK = 2048
FORMAT = pyaudio.paInt16
CHANNELS = 1
RATE = 16000 # the same sample rate as the librispeech dataset

if __name__ == "__main__":
    p = pyaudio.PyAudio()
    process = Process()
    frames = []
    recordLength = int(sys.argv[2])
    playback = eval(sys.argv[3])
    
    print("Loading model")
    model = load_model(sys.argv[1], custom_objects = {'ctcloss': ASRModel.ctcloss}, safe_mode = False)

    input("\nPress enter when you are ready to start recoding...")
    
    stream = p.open(
        format            = FORMAT,
        channels          = CHANNELS,
        rate              = RATE,
        input             = True,
        frames_per_buffer = CHUNK
    )
    
    # Open a stream to play the audio 
    outStream = p.open(
        format=FORMAT, 
        channels=CHANNELS,
        rate=RATE, 
        output=True
    )

    print("\nRecording...")
    
    # Record the audio sample from the users microphone
    for i in range(0, int(RATE / CHUNK * recordLength)):
        data = stream.read(CHUNK)
        frames.append(np.frombuffer(data, dtype = np.int16))
    
    print("Done recording")
    # audioData = b''.join(frame.tobytes() for frame in frames)
    audioData = np.concatenate(frames)

    # Normalize the audio sample
    audioData = audioData.astype(np.float32) / np.max(np.abs(audioData))
    
    # RMS Normalization for conistant loadness in tha audio sample
    target_rms = 0.1
    rms = np.sqrt(np.mean(audioData**2))
    audioData = audioData * (target_rms / rms)
    audioData = (audioData * 32767).astype(np.int16)

    if (playback):
        print("\nPlaying back...")
        for i in range(0, len(audioData), CHUNK * 2):
            outStream.write(audioData[i:i + CHUNK * 2].tobytes())

    # Conver to a tensor
    audio = tf.convert_to_tensor(audioData, dtype = tf.float32)
    audio = audio / np.max(np.abs(audio))
    audio = tf.reshape(audio, [1, -1])
    
    # Compute the log scale spectrogram
    spectrogram = tf.signal.stft(audio, frame_length = 256, frame_step = 160, fft_length = 384)
    spectrogram = tf.abs(spectrogram)
    spectrogram = tf.math.pow(spectrogram, 0.5)
    
    # Normalize the spectrogram
    means = tf.math.reduce_mean(spectrogram, 1, keepdims = True)
    std = tf.math.reduce_std(spectrogram, 1, keepdims = True)
    spectrogram = (spectrogram - means) / (std + 1e-10)
    
    # Get the prediction from the model
    sentiment = model.predict(spectrogram)
    label = ASRModel.ctcDecoder(sentiment)
    prediction = process.ConvertLabel(label)

    print("\nYou said:")
    print(prediction)

    frames.clear()
    
    stream.stop_stream()
    stream.close()
    outStream.stop_stream()
    outStream.close()
    p.terminate()
