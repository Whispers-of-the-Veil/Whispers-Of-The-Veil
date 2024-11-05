from Data.Preporcess import Process
import sys

process = Process()

files, transcripts = process.LoadCSV(sys.argv[1])
spectrograms, specShape = process.Audio(files, 0)
labels, numClasses = process.Transcript(transcripts, 0)

print(f"Shape of the spectrograms {spectrograms.shape}")
print(f"Shape of the labels {labels.shape}")