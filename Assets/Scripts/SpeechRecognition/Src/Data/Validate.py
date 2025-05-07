# Lucas

import numpy as np
import matplotlib.pyplot as plt

from Data.Process import Process

from Grab_Ini import ini

class Validate:
    def __init__(self, process: Process):
        self.process = process

        generalConfig    = ini().grabInfo("config.ini", "General")
        seed = int(generalConfig['seed'])

        np.random.seed(seed)

    def PlotSpec(self, _spec):
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
        plt.title(f"Spectrogram")
        plt.xlabel('Frames')
        plt.ylabel('Mel Frequency Bins')

        plt.show()
    
    def Spectrogram(self, _spec, _label):
        """
        A helper function used to display the processed Spectrograms and Labels.
        Given the amount of samples the user wishes to show, it will randomly
        select those samples and print the labels to the screen, the spectrograms
        will be passed to a helper function that will display them to the user.

        Parameters:
            - _specs: A list containing the spectrograms to view
            - _labels: A list containing the labels to view
        """
        trans = self.process.ConvertLabel(_label)

        self.PlotSpec(_spec)

        print(f"Spec Shape: {_spec.shape}")
        print(f"Labels: {_label}")
        print(f"Transcripts: {trans}")