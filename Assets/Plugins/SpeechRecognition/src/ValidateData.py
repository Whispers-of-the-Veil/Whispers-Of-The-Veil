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

def TestShape(_shape, _nextShape):
    try:
        assert _shape == _nextShape, f"The shape {_shape} doesn't match {_nextShape}"     # Determine if the shapes of the spectrograms are the same
    except Exception as e:
        print(f"An error occurred during testing: {e}")

def TestLength(_labels):
    try:
        assert len(_labels[i]) == len(_labels[i + 1]), f"The length of the labels {len(_labels[i])} | {len(_labels[i + 1])} Doesn't match"
    except Exception as e:
        print(f"An error occurred during testing: {e}")

if __name__ == "__main__":
    data = np.load(sys.argv[1])  # Load the processed data
    spectrogram = data['Spectrograms']
    labels = data['Labels']
    shape = data['InputShape']

    # Visualize some spectrograms
    for i in range(20):  # Visualize the first 5 spectrograms
        plot_spectrogram(spectrogram[i], title=f'Spectrogram of Sample {i}')                    # Visually show the spectrogram
        print(f"Labels: {labels[i]}")                                                           # Show the labels
        TestLength(labels)
        TestShape(spectrogram[i].shape[1:], spectrogram[i + 1].shape[1:])
