# Lucas Davis

import librosa
import numpy as np
import tensorflow as tf
import matplotlib.pyplot as plt

FILE_PATH = "/home/ldavis/Code/Data_Sets/SpeechRec/LibriSpeech/train-clean-100/19/198/19-198-0001.wav"

def _PlotSpec(_spec, _index):
    """
    This function will use matplot to display the spectrograms to the screen

    Parameters:
        - _spec: An individual spectrogram to show
        - _index: The index number of that spectrogram
    """
    spectrogram = np.squeeze(_spec)

    plt.figure(figsize=(25, 10))
    plt.imshow(spectrogram, aspect = 'auto', origin = 'lower', cmap = 'jet')
    plt.colorbar(format='%+2.0f dB')
    plt.title(f"Spectrogram of sample {_index}")
    plt.xlabel('Frames')
    plt.ylabel('Mel Frequency Bins')

    plt.show()

def _ProcessAudioFile(_file):
    """
    Read an audio file, compute its Mel spectrogram, normalize it, and pad/truncate it to the target length.

    Parameters:

    Returns:
        The normalized Mel spectrogram for the given audio file
    """
    audioSample, sr = librosa.load(_file, sr = 16000)

    stft = librosa.stft(audioSample, win_length = 256, hop_length = 160, n_fft = 94)
    spectrogram = np.abs(stft)

    means = tf.math.reduce_mean(spectrogram, 1, keepdims=True)
    stddevs = tf.math.reduce_std(spectrogram, 1, keepdims=True)
    spectrogram = (spectrogram - means) / (stddevs + 1e-10)

    # windowlength = int(0.025 * sr)
    # hopLength = int(0.010 * sr)

    # spectrogram = librosa.feature.melspectrogram(y = magnitude, sr = sr, n_mels = 193, hop_length = hopLength, win_length = windowlength)

    # logSpectrogram = librosa.power_to_db(spectrogram, ref = np.max)

    return _Normalize(spectrogram)
    
def _Normalize(_melSpectrogram):
    """
    Normalizes the Mel spectrogram using z-score normalization.
    
    Parameters:
        - _melSpectrogram: The computed Mel spectrogram (2D NumPy array)
        
    Returns:
        - Normalized spectrogram with mean 0 and standard deviation 1
    """
    # Center the spectrogram by subtracting the mean
    centeredSpectrogram = _melSpectrogram - np.mean(_melSpectrogram)
    
    # Scale the centered spectrogram to [-1, 1] by dividing by the max absolute value
    maxAbsValue = np.max(np.abs(centeredSpectrogram))
    normalizedSpectrogram = centeredSpectrogram / maxAbsValue if maxAbsValue != 0 else centeredSpectrogram
    
    return normalizedSpectrogram

def tfSpec(_filePath):
    # 1. Read wav file
    file = tf.io.read_file(_filePath)
    # 2. Decode the wav file
    audio, _ = tf.audio.decode_wav(file)
    audio = tf.squeeze(audio, axis=-1)

    targetLength = int(18 * 16000) - 1
    if len(audio) < targetLength:
        padding = targetLength - len(audio)
        audio = np.concatenate((audio, np.zeros(padding, dtype=np.float32)))
    elif len(audio) > targetLength:
        audio = audio[:targetLength]  # Truncate if necessary

    # 3. Change type to float
    audio = tf.cast(audio, tf.float32)

    # 4. Get the spectrogram
    spectrogram = tf.signal.stft(
        audio, frame_length = 256, frame_step = 160, fft_length = 384
    )
    # 5. We only need the magnitude, which can be derived by applying tf.abs
    spectrogram = tf.abs(spectrogram)
    spectrogram = tf.math.pow(spectrogram, 0.5)
    # 6. normalisation
    means = tf.math.reduce_mean(spectrogram, 1, keepdims=True)
    stddevs = tf.math.reduce_std(spectrogram, 1, keepdims=True)
    spectrogram = (spectrogram - means) / (stddevs + 1e-10)

    return spectrogram

if __name__ == "__main__":
    mySpectrogram    = _ProcessAudioFile(FILE_PATH)
    guideSpectrogram = tfSpec(FILE_PATH)

    print(f"My spectrograms look like: {mySpectrogram.shape}")
    _PlotSpec(mySpectrogram, 0)

    print(f"Their spectrogram looks like: {guideSpectrogram.shape}")
    _PlotSpec(guideSpectrogram, 0)