import numpy as np
import h5py
import matplotlib.pyplot as plt
import librosa
import sys

def PlotGraphs(_mfcc, _spectrograms, _sampleRate, title):
    # Squeeze to remove dimensions of size 1
    spectrogram = np.squeeze(_spectrograms)

    plt.figure(figsize=(25, 10))

    plt.subplot(2, 1, 1)
    librosa.display.specshow(_mfcc, sr = _sampleRate, x_axis = 'time')
    plt.colorbar(format='%+2f')
    plt.title('MFCC')

    plt.subplot(2, 1, 2)
    plt.imshow(spectrogram, aspect = 'auto', origin = 'lower', cmap = 'jet')
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
    # Load the processed data from the HDF5 file
    with h5py.File(sys.argv[1], 'r') as data:
        spectrograms = data['Spectrograms'][:]
        mfccs = data['MFCC'][:]
        labels = data['Labels'][:]

        specShape = tuple(data['SpecShape'][:])
        inputShape = tuple(data['InputShape'][:])
        outputSize = int(data['OutputSize'][()])
        sampleRate = int(data['SampleRate'][()])
    
    print(f"Spectrograms shape: {specShape}")
    print(f"Input shape of the model (MFCC): {inputShape}")
    print(f"Shape of the labels: {labels.shape}")
    print(f"Output size (num of classes): {outputSize}")
    print(f"Sample Rate: {sampleRate} | {type(sampleRate)}")

    print(f"Size of Spectrograms {sys.getsizeof(spectrograms)}")
    print(f"Size of Labels {sys.getsizeof(labels)}")
    
    # Visualize some spectrograms
    for i in range(50):  # Visualize the first 20 spectrograms
        print(f"MFCC: {mfccs[i]}")
        PlotGraphs(mfccs[i], spectrograms[i], sampleRate, title=f'MFCC and Spectrograms of Sample {i}')  # Visually show the spectrogram
        print(f"Labels: {labels[i]}")  # Show the labels
        TestLength(labels)
        TestShape(spectrograms[i].shape[1:], spectrograms[i + 1].shape[1:])

