from DeepSpeechAPI import DeepSpeechAPI
import librosa
import soundfile as sf
import numpy as np

api = DeepSpeechAPI()
file = "/home/ldavis/Code/Data_Sets/SpeechRec/LibriSpeech/train-clean-100/19/198/19-198-0000.wav"

audioSample, sampleRate = sf.read(file, dtype='float32')

windowLength = int(0.025 * sampleRate)
hopLength = int(0.010 * sampleRate)

spectrogram = librosa.stft(audioSample, n_fft = windowLength, hop_length = hopLength).T

print(spectrogram.shape)

magnitude, _ = librosa.magphase(spectrogram)
melScaleSpectrogram = librosa.feature.melspectrogram(S = magnitude, sr = sampleRate, n_fft = windowLength, hop_length = hopLength)

# Interpolate to extend the frequency bins to 500
mel_spectrogram_extended = np.interp(
    np.linspace(0, melScaleSpectrogram.shape[0], 500),
    np.arange(melScaleSpectrogram.shape[0]),
    melScaleSpectrogram
)

logMelSpectrogram = librosa.amplitude_to_db(mel_spectrogram_extended, ref = np.max)

output = api.Predict(logMelSpectrogram)

print(output)