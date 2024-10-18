import numpy as np
import librosa

import sys

def plot_spectrogram(spectrogram, title='Spectrogram'):
    # Squeeze to remove dimensions of size 1
    spectrogram = np.squeeze(spectrogram)

    librosa.display.specshow(spectrogram, sr = sample_rate, x_axis = 'time', y_axis = 'mel')

if __name__ == "__main__":
    data = np.load(sys.argv[1])  # Load the processed data
    spectrogram = data['Spectrograms']

    # Visualize some spectrograms
    for i in range(5):  # Visualize the first 5 spectrograms
        plot_spectrogram(spectrogram[i], title=f'Spectrogram of Sample {i}')