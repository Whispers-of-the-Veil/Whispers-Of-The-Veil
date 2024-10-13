import tensorflow as tf
import numpy as np
import pandas as pd

import sys

class Preprocess:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self): pass

    def AudioData(self, _audioFiles, _targetLength):
        """
        Loads audio files, computes their Mel spectrograms, and pads/truncates them to a uniform length

        Parameters:
            - _audioFiles: A list of file paths for the audio files
            - _targetLength: The target length for the spectrograms

        Returns:
            - A NumPy array of processed audio data
        """
        audioData = []

        # Load and preprocess each audio file
        for x, file in enumerate(_audioFiles):
            progressValue = ((x + 1) / len(_audioFiles)) * 100

            audio, sampleRate = self.ProcessAudio(file)

            # Extract Mel spectrogram from the audio
            melSpectrogram = self.ComputeMelSpectrogram(audio, sampleRate)
            paddedSpectrogram = self.PadSpectrograms([melSpectrogram], _targetLength)[0]   

            audioData.append(paddedSpectrogram.numpy())  # Convert tensor to numpy array

            print(f"{progressValue}%")

        return audioData
    
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

        return transcript

    def NormalizeAudio(self, _audio):
        """
        Normalizes the audio signal to have a maximum value of 1 by dividing by the maximum absolute value.

        Parameters:
            - _audio: An audio tensor

        Returns:
            The normalized audio tensor
        """
        return _audio / tf.reduce_max(tf.abs(_audio))

    def ProcessAudio(self, _filePath):
        """
        Load and preprocess audio from a file.
        
        Parameter:
            - _filePath: The file path to the audio file

        Returns:
            A normalizeed audio tensor and its sameple rate
        """
        # Load the audio file
        audioBinary = tf.io.read_file(_filePath)

        # Decode the WAV file into a tensor
        audio, sampleRate = tf.audio.decode_wav(audioBinary)
        audio = tf.squeeze(audio, axis=-1)  # Remove any unnecessary dimensions

        return self.NormalizeAudio(audio), sampleRate
    
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

    process = Preprocess()
    targetLength = 100  # Used to pad the Spectrograms

    # Set the seed value for experiment reproducibility.
    seed = 42
    tf.random.set_seed(seed)
    np.random.seed(seed)

    print(f"Loading data from the csv file: {sys.argv[1]}")
    audioFiles, transcript = LoadCSV(sys.argv[1])

    print("Preprocessing the data")
    audioData = process.AudioData(audioFiles, targetLength)
    transcriptData = process.Transcript(transcript)

    print(f"Saving training data to {sys.argv[2]}")
    np.savez_compressed(sys.argv[2], audioData = audioData, transcriptData = transcriptData)

    print(f"training data set saved to {sys.argv[2]}")
    print("Closing script")