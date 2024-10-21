import numpy as np
import pandas as pd
import soundfile as sf
import librosa

import concurrent.futures
import threading
from tqdm import tqdm  # For progress bar

import tensorflow as tf
from tensorflow.keras.preprocessing.sequence import pad_sequences

import sys
import os
import glob

class Process:
    """
    This class contains methods to handle the audio data and transcripts
    """
    def __init__(self):
        self.lock = threading.Lock()  # Create a lock for thread safety

    # ------------------------------------------------------------------------
    #   Audio processing
    # ------------------------------------------------------------------------

    def Audio(self, _audioFiles, _targetLength, _batchSize, _outFile):
        index = 1
        
        with tqdm(total = len(_audioFiles)) as pbar:
            # Divide audio files into batches
            for i in range(0, len(_audioFiles), _batchSize):
                batchFiles = _audioFiles[i:i + _batchSize]
                
                # Use ProcessPoolExecutor for CPU-bound tasks
                with concurrent.futures.ThreadPoolExecutor(max_workers = 5) as executor:
                    futureSpectrograms = {
                        executor.submit(self.BatchAudioHelper, batchFiles, _targetLength): batchFiles
                    }
                    
                    for future in concurrent.futures.as_completed(futureSpectrograms):
                        try:
                            spectrograms = future.result()

                            self.SaveBatch(spectrograms, index, _outFile)

                            index += 1

                            # Update progress bar
                            pbar.update(len(spectrograms))  
                        except Exception as e:
                            print(f"Error processing batch {futureSpectrograms[future]}: {e}")

    def SaveBatch(self, _spectrograms, _index, _outFile):
        """Write spectrograms to a specified path."""
        # Ensure the output directory exists
        os.makedirs(_outFile, exist_ok=True)
        
        # Define the path for the temporary file
        tempfile_path = os.path.join(_outFile, f'tmpfile_batch{_index}.dat')
        
        # Acquire lock to ensure thread-safe writing
        with self.lock:
            # Create a memmap array
            batchMemmap = np.memmap(tempfile_path, dtype='float32', mode='w+', shape=(len(_spectrograms), 128, 1000))
            
            for j, spectrogram in enumerate(_spectrograms):
                batchMemmap[j] = spectrogram

    def BatchAudioHelper(self, batchFiles, _length):
        spectrograms = []

        for _file in batchFiles:
            audioSample, sampleRate = sf.read(_file, dtype='float32')
            melSpectrogram = self.ComputeMelSpectrogram(audioSample, sampleRate, _length)
            normalizedSpectrogram = self.NormalizeZScore(melSpectrogram)
            paddedSpectrogram = self.PadSpectrograms(normalizedSpectrogram, _length)
            spectrograms.append(paddedSpectrogram)

            del melSpectrogram, normalizedSpectrogram  # Explicitly free memory
        
        return spectrograms
    
    def ComputeMelSpectrogram(self, _audioSample, _sampleRate, _length):
        """
        Compute the Mel spectrogram of the audio signal.
        
        Parameters:
            - _audio: An audio tensor
            - _sampleRate: The sample rate of that audio tensor

        Returns:
            - A 2D NumPy array representing the Mel spectrogram
        """
        # Compute the Short-Time Fourier Transform (STFT)
        spectrogram = librosa.stft(_audioSample)

        # Compute magnitude and phase, use magnitude only
        magSpectrogram, _ = librosa.magphase(spectrogram)

         # Convert to Mel spectrogram using the Mel scale
        melScaleSpectrogram = librosa.feature.melspectrogram(S = magSpectrogram, sr = _sampleRate, center=True, pad_mode = _length)

        # Convert amplitude to decibels
        melSpectrogram = librosa.amplitude_to_db(melScaleSpectrogram, ref = np.min)

        return melSpectrogram
    
    def NormalizeZScore(self, _melSpectrogram):
        """
        Normalizes the Mel spectrogram using z-score normalization.
        
        Parameters:
            - _melSpectrogram: The computed Mel spectrogram (2D NumPy array)
            
        Returns:
            - Normalized spectrogram with mean 0 and standard deviation 1
        """
        mean = np.mean(_melSpectrogram)
        std = np.std(_melSpectrogram)
        
        return (_melSpectrogram - mean) / std
    
    def PadSpectrograms(self, _melSpectrogram, _targetLength):
        """
        Pads or truncates the Mel spectrogram to the target length.

        Parameters:
            - melSpectrogram: The computed Mel spectrogram (2D NumPy array)
            - target_length: The desired length in frames

        Returns:
            - The padded or truncated spectrogram
        """
        if _melSpectrogram.shape[1] > _targetLength:
            # Truncate the spectrogram
            return _melSpectrogram[:, :_targetLength]
        elif _melSpectrogram.shape[1] < _targetLength:
            # Pad the spectrogram with zeros
            pad_width = _targetLength - _melSpectrogram.shape[1]

            return np.pad(_melSpectrogram, ((0, 0), (0, pad_width)), mode = 'constant')
        
        return _melSpectrogram

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
    
def LoadSpectrogramsFromDatFiles(_directory):
    """Load all .dat files from the specified directory into a single compressed NumPy array efficiently."""
    # Get the list of .dat files in sorted order
    files = sorted([f for f in os.listdir(_directory) if f.endswith('.dat')])
    
    # Initialize the memmap array for concatenation
    firstFile = os.path.join(_directory, files[0])
    firstMemMap = np.memmap(firstFile, dtype='float32', mode='r')
    
    # Determine the shape from the first file
    numBatches = firstMemMap.shape[0]
    
    # Create an empty memmap for the final output
    totalShape = (len(files) * numBatches, 128, 1000)
    spectrograms = np.memmap('combined_spectrograms.dat', dtype = 'float32', mode = 'w+', shape = totalShape)

    index = 0
    
    for file in files:
        filePath = os.path.join(_directory, file)
        
        # Load each memmap array and copy its data to the final memmap
        memmap_array = np.memmap(filePath, dtype='float32', mode='r')
        num_batches = memmap_array.shape[0]
        
        # Reshape and assign to the final memmap array
        spectrograms[index:index + numBatches] = memmap_array.reshape((numBatches, 128, 1000))
        index += numBatches

    return spectrograms

def CleanDataFiles(directory):
    # Create a pattern for .dat files
    pattern = os.path.join(directory, '*.dat')
    
    # Find all .dat files in the specified directory
    for file_path in glob.glob(pattern):
        try:
            os.remove(file_path)  # Remove the file
            print(f"Removed: {file_path}")  # Optional: print the removed file path
        except Exception as e:
            print(f"Error removing {file_path}: {e}")

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

    # Config
    targetLength = 1000
    seed = 42
    batchSize = 2000
    outFile = "tmpData"

    process = Process()

    # Set the seed value for experiment reproducibility.
    tf.random.set_seed(seed)
    np.random.seed(seed)

    print(f"Loading data from the csv file: {sys.argv[1]}")
    audioFiles, transcript = LoadCSV(sys.argv[1])

    print("\nProcessing Audio files")
    process.Audio(audioFiles, targetLength, batchSize, outFile) # Shape: (batch_size, height, width, 1)
    
    print("\nProcessing the Transcripts")
    labels, size = process.Transcript(transcript)

    print("Done")

    # Save the Spectrogram of the audio file, shape of the spectrogram (timesteps, frequency bins, channels),
    #  the size of the output layer (number of unique characters), and the Labels
    print(f"\nSaving training data to {sys.argv[2]}")
    spectrograms = LoadSpectrogramsFromDatFiles(outFile)

    np.savez_compressed(sys.argv[2], Spectrograms = spectrograms, Labels = labels, InputShape = spectrograms.shape[1:], OutputSize = size)

    CleanDataFiles(outFile)