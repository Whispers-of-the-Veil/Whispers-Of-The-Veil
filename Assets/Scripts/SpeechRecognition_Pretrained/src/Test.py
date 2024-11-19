from DeepSpeech import API
import librosa

api = API()
file = "/home/ldavis/Code/Data_Sets/SpeechRec/LibriSpeech/TestSet/61/70968/61-70968-0000.wav"

audioSample, sr = librosa.load(file, sr = 16000)

predictedTranscript = api.Predict(audioSample, sr)

print(f"The transcript of {file}:\n{predictedTranscript}")