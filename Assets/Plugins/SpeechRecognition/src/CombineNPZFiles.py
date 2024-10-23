import numpy as np
import sys
import os

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("usage: python PreProcessing.py /path/to/directory/holding/datafiles")
        print("Data should have the form of: filePath, fileSize, transcript")
        exit(1)

    allSpectrograms = []
    allLabels = []

    for i, file in enumerate(sorted(os.listdir(sys.argv[1]))):
        data = np.load(os.path.join(sys.argv[1], file))

        allSpectrograms.append(data['Spectrograms'])
        allLabels.append(data['Labels'])
    
    combinedSpectrograms = np.concatenate(allSpectrograms, axis = 0)
    combinedLabels = np.concatenate(allLabels, axis = 0)

    

