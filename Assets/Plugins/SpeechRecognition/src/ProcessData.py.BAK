import numpy as np
import pandas as pd
import math

import tensorflow as tf
from tensorflow.keras.preprocessing.sequence import pad_sequences

import sys

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self): pass

    # ------------------------------------------------------------------------
    #   Audio processing
    #       - Decode the Wav files into tensors 
    #       - Compute a mel spectrogram from the audio tensors
    #       - Padd the spectrograms
    # ------------------------------------------------------------------------

    def Audio(self, _audioFiles, _targetLength):
        """
        Loads audio files, computes their Mel spectrograms, and pads/truncates them to a uniform length

        Parameters:
            - _audioFiles: A list of file paths for the audio files
            - _targetLength: The target length for the spectrograms

        Returns:
            - A NumPy array of processed audio data
        """
        spectrogram = []

        # Load and preprocess each audio file
        for x, file in enumerate(_audioFiles):
            audioBinary = tf.io.read_file(file)

            # Decode the WAV file into a tensor
            audio, sampleRate = tf.audio.decode_wav(audioBinary)
            audio = tf.squeeze(audio, axis = -1)  # Remove any unnecessary dimensions
            audio = self.NormalizeAudio(audio)

            # Extract Mel spectrogram from the audio
            melSpectrogram = self.ComputeMelSpectrogram(audio, sampleRate)
            normalizedMelSpectrogram = self.LogAndNormalizeMelSpectrogram(melSpectrogram)
            paddedSpectrogram = self.PadSpectrograms([normalizedMelSpectrogram], _targetLength)[0]

            # Convert the spectrogram tensor to numpy array
            spectrogram.append(paddedSpectrogram.numpy())  
            
            # Display the progress
            progressValue = ((x + 1) / len(_audioFiles)) * 100
            if math.floor(progressValue) > math.floor((x / len(_audioFiles)) * 100):
                print(f"{math.floor(progressValue)}%")

        return spectrogram

    def NormalizeAudio(self, _audio):
        """
        Normalizes the audio signal to have a maximum value of 1 by dividing by the maximum absolute value.

        Parameters:
            - _audio: An audio tensor

        Returns:
            The normalized audio tensor
        """
        return _audio / tf.reduce_max(tf.abs(_audio))
    
    def ComputeMelSpectrogram(self, _audio, _sampleRate):
        """
        Compute the Mel spectrogram of the audio signal.
        
        Parameters:
            - _audio: An audio tensor
            - _sampleRate: The sample rate of that audio tensor

        returns:
            A Mel Spectrogram Tensor
        """
        # Define parameters for Mel spectrogram
        numMelBins = 64
        frameLength = 256
        frameStep = 128
        
        # Compute STFT
        stft = tf.signal.stft(_audio, frame_length = frameLength, frame_step = frameStep)
        spectrogram = tf.abs(stft)
        
        # Create a Mel filter
        melFilter = tf.signal.linear_to_mel_weight_matrix(
            numMelBins, 
            spectrogram.shape[-1], 
            _sampleRate
        )
        
        # Apply Mel filter to the spectrogram
        melSpectrogram = tf.tensordot(spectrogram, melFilter, 1)
        melSpectrogram.set_shape(spectrogram.shape[:-1].concatenate(melFilter.shape[-1:]))
        
        return melSpectrogram
    
    def LogAndNormalizeMelSpectrogram(self, _melSpectrogram):
        """
        Nomalizes a Mel Spectrogram by computing both the LogScal and zero mean / unit variance

        Parameters:
            - melSpectrogram: The Mel spectrogram tensor.
        
        returns:
            A normalized Mel Spectrogram
        """
        # Apply log scaling
        logMelSpectrogram = self.LogScaleMelSpectrogram(_melSpectrogram)
        
        # Normalize (mean = 0, std = 1)
        normalizedSpectrogram = self.NormalizeMelSpectrogram(logMelSpectrogram)
        
        return normalizedSpectrogram
    
    def LogScaleMelSpectrogram(self, _melSpectrogram):
        """
        Applies log scaling to the Mel spectrogram (adds a small value to avoid log(0)).
        
        Parameters:
            - melSpectrogram: The Mel spectrogram tensor.
        
        Returns:
            - The log-scaled Mel spectrogram tensor.
        """
        logMelSpectrogram = tf.math.log(_melSpectrogram + 1e-6)  # Adding a small constant to avoid log(0)

        return logMelSpectrogram
    
    def NormalizeMelSpectrogram(self, _melSpectrogram):
        """
        Standardizes the Mel spectrogram to have zero mean and unit variance.
        
        Parameters:
            - melSpectrogram: The Mel spectrogram tensor.
        
        Returns:
            - The normalized Mel spectrogram tensor.
        """
        mean = tf.reduce_mean(_melSpectrogram)
        stddev = tf.math.reduce_std(_melSpectrogram)
        normalizedSpectrogram = (_melSpectrogram - mean) / stddev

        return normalizedSpectrogram
    
    def PadSpectrograms(self, _spectrograms, _targetLength):
        """Pad or truncate the spectrograms to ensure they all have the same length."""
        paddedSpectrogram = []

        for spectrogram in _spectrograms:
            if spectrogram.shape[0] < _targetLength:
                # Pad with zeros if shorter than target_length
                padding = tf.zeros((_targetLength - spectrogram.shape[0], spectrogram.shape[1]))
                padded_spectrogram = tf.concat([spectrogram, padding], axis=0)
            else:
                # Truncate if longer than target_length
                padded_spectrogram = spectrogram[:_targetLength]

            paddedSpectrogram.append(padded_spectrogram)

        return paddedSpectrogram
    
    # ------------------------------------------------------------------------
    #   Transcript processing
    #       - Make the sure the transcripts are lower case. And all punctuation
    #         is removed
    #       - Create a dictionary mapping of characters to integers
    #       - Prepare the labels
    #           - Transform the transcripts into their repective indices
    #           - Pad the labels
    # ------------------------------------------------------------------------
    
    def Transcript(self, _transcripts):
        """
        Cleans and normalizes the transcripts.
        
        Parameters:
            - _transcripts: A list of transcripts

        Returns:
            A list of cleaned transcripts
        """
        # Example preprocessing steps
        transcript = []

        for trans in _transcripts:
            # Lowercase the transcript
            trans = trans.lower()
            # Remove punctuation
            trans = ''.join(c for c in trans if c.isalnum() or c.isspace())

            transcript.append(trans)

        charToIndex = self.CreateVocabulary(transcript)

        maxLength = max(len(t) for t in transcript)
        labels = self.PrepareLabels(transcript, charToIndex, maxLength)

        size = len(set(''.join(transcript))) + 1

        return labels, size
    
    def CreateVocabulary(self, _transcripts):
        """
        Create a dictionary mapping of characters to integer from a list of transcripts.

        Parameters:
            - _trainscripts: A list of transcripts
        
        Returns:
            returns a chracter-to-indice dictionary
        """
        uniqueChar = sorted(set(''.join(_transcripts)))  # Unique characters in all transcripts
        charIndex = {char: index + 1 for index, char in enumerate(uniqueChar)}  # Indexing starting from 1
        charIndex['<PAD>'] = 0  # Adding a padding character

        return charIndex

    def PrepareLabels(self, _transcripts, _charIndex, _maxLength):
        """
        Takes in a list of transcripts, converts them into their corresponding character indices 
        using a provided character index dictionary, and pads them to ensure they all have the same length

        Parameters:
            - _transcript: A list of transcripts
            - _charIndex: A dictionary mapping characters to their respective indices
            - _maxLength: The maximum length of the sequences after padding

        Returns:
            Returns a list of processed transcripts
        """
        indexedTranscripts = self.TranscriptToIndices(_transcripts, _charIndex)  # Convert to indices

        # Ensure that the input is a list of lists
        if not all(isinstance(i, list) for i in indexedTranscripts):
            raise ValueError("Indexed transcripts should be a list of lists.")

        padded_labels = pad_sequences(indexedTranscripts, maxlen = _maxLength, padding = 'post', value = _charIndex['<PAD>'])  # Use index 0 for padding

        return padded_labels

    def TranscriptToIndices(self, _transcripts, _charIndex):
        """
        Converts the text transcripts into a list of integer indices using a provided a dictionary of chracter mappings

        Parameters:
            - _trascripts: A list of transcripts
            - _charIndex: A dictionary mapping characters to their respective indices
        
        Returns:
            A lists containing the integer indices for each transcript
        """
        indexedTranscripts = []

        for transcript in _transcripts:
            # Convert each character in the transcript to its corresponding index
            indexedTranscript = [_charIndex[char] for char in transcript if char in _charIndex]  # Ensure each char exists in the mapping
            indexedTranscripts.append(indexedTranscript)  # Append the list of indices to the main list

        return indexedTranscripts

def LoadCSV(_csvPath):
    """
    Loads audio files paths and transcripts from a CSV file in the form of:
	    wave_filename(path to the audio file), wave_filesize, transcript

    Parameters:
        - _csvPath: The path to the CSV file

    Returns:
        Two lists: one for audio paths and one for transcripts
    """
    data = pd.read_csv(_csvPath)

    audioPath = data['wav_filename'].tolist()
    transcripts = data['transcript'].tolist()

    return audioPath, transcripts

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("usage: python PreProcessing.py /path/to/data.csv outputfile.npz")
        print("Data should have the form of: filePath, fileSize, transcript")
        exit(1)

    process = Process()

    # Config
    targetLength = 100
    seed = 42

    # Set the seed value for experiment reproducibility.
    tf.random.set_seed(seed)
    np.random.seed(seed)

    print(f"Loading data from the csv file: {sys.argv[1]}")
    audioFiles, transcript = LoadCSV(sys.argv[1])

    print("Processing Audio files")
    spectrograms = np.expand_dims(process.Audio(audioFiles, targetLength), axis = -1) # Shape: (batch_size, height, width, 1)
    inputShape = spectrograms.shape[1:]
    
    print("\nProcessing the Transcripts")
    labels, size = process.Transcript(transcript)

    print("Done")

    # Save the Spectrogram of the audio file, shape of the spectrogram (timesteps, frequency bins, channels),
    #  the size of the output layer (number of unique characters), and the Labels
    print(f"\nSaving training data to {sys.argv[2]}")
    np.savez_compressed(sys.argv[2], Spectrograms = spectrograms, Transcript = transcript, InputShape = inputShape, OutputSize = size, Labels = labels)