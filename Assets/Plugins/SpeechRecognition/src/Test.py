import numpy as np

data = np.load('/home/ldavis/Code/Whispers-Of-The-Veil/Assets/Plugins/SpeechRecognition/Data/100/100_Batch1.npz')
array = data['Spectrograms']

size_in_bytes = array.nbytes

print(f'Size of the uncompressed array in memory: {size_in_bytes} bytes')
