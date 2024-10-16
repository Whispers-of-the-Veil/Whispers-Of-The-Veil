import numpy as np
import matplotlib.pyplot as plt

import sys

def plot_spectrogram(spectrogram, title='Spectrogram'):
    # Squeeze to remove dimensions of size 1
    spectrogram = np.squeeze(spectrogram)
    
    plt.figure(figsize=(10, 4))
    plt.imshow(spectrogram.T, aspect='auto', origin='lower', cmap='jet')
    plt.colorbar(format='%+2.0f dB')
    plt.title(title)
    plt.xlabel('Frames')
    plt.ylabel('Mel Frequency Bins')
    plt.show()

if __name__ == "__main__":
    data = np.load(sys.argv[1])  # Load the processed data
    spectrogram = data['Spectrograms']

    # Visualize some spectrograms
    for i in range(5):  # Visualize the first 5 spectrograms
        plot_spectrogram(spectrogram[i], title=f'Spectrogram of Sample {i}')