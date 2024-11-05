from DeepSpeech import API
import soundfile as sf

api = API()
file = "/Users/lucasdavis/Code/Data/librispeech/Test/61/70968/61-70968-0000.wav"

audioSample, sampleRate = sf.read(file, dtype='float32')
predictedTranscript = api.Predict(audioSample, sampleRate)

print(f"The transcript of {file}:\n{predictedTranscript}")