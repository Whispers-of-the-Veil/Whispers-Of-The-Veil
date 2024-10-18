import librosa.display
import librosa
import matplotlib.pyplot as plt

from scipy.io import wavfile
import numpy as np

AUDIO_FILE = '/Users/lucasdavis/Code/Whispers-of-the-Veil/Assets/Plugins/SpeechRecognition/librispeech/train-clean-100/211/122425/211-122425-0000.wav'

sample_rate, samples = wavfile.read(AUDIO_FILE)

samples = samples.astype(np.float32) / np.max(np.abs(samples))

print(f"Samples {samples} | Same_rate {sample_rate}")

sgram = librosa.stft(samples)
sgram_mag, _ = librosa.magphase(sgram)
mel_scale_sgram = librosa.feature.melspectrogram(S=sgram_mag, sr=sample_rate)
melScaleSpectrogram = librosa.feature.melspectrogram(S = sgram_mag, sr = sample_rate, center=True, pad_mode = 100)

mel_sgram = librosa.amplitude_to_db(mel_scale_sgram, ref=np.min)
melSpectrogram = librosa.amplitude_to_db(melScaleSpectrogram, ref=np.min)

# librosa.display.specshow(mel_sgram, sr=sample_rate, x_axis='time', y_axis='mel')
# plt.colorbar(format='%+2.0f dB')

librosa.display.specshow(melSpectrogram, sr=sample_rate, x_axis='time', y_axis='mel')
plt.colorbar(format='%+2.0f dB')

# Ensure the plot stays open
plt.show()