import os
import csv
import sys

def ProcessFile(_rootDir, _outputCSV):
    """
    This will walk through a root directory and all sub directories looking for two types of files: .wav files and .trans.txt.
    The .trans.txt contains the transcript of all the audio files in a given directory.

    Parameters:
        - _rootDir: The root directory where audio files and transcript files are located.
        - _outputCSV: The name of the output CSV file where the processed information will be stored.
    """
    # Open the output CSV file for writing
    with open(_outputCSV, mode='w', newline='', encoding='utf-8') as csvFile:
        csv_writer = csv.writer(csvFile)
        # Write the CSV header
        csv_writer.writerow(["wav_filename", "wav_filesize", "transcript"])

        # Walk through all subdirectories and files in the root directory
        for subdir, _, files in os.walk(_rootDir):
            # Look for .trans.txt files
            for file in files:
                if file.endswith(".trans.txt"):
                    transFilePath = os.path.join(subdir, file)

                    # Process the .trans.txt file
                    with open(transFilePath, 'r', encoding='utf-8') as transcriptFile:
                        for line in transcriptFile:
                            # Extract the wav file name (without extension) and transcript
                            parts = line.strip().split(' ', 1)
                            if len(parts) == 2:
                                wavefile = parts[0]
                                transcript = parts[1]

                                # Find the corresponding .wav file
                                wav_filename = os.path.join(subdir, wavefile + ".wav")
                                if os.path.exists(wav_filename):
                                    wav_filesize = os.path.getsize(wav_filename)

                                    # Write to CSV: wav_filename, wav_filesize, transcript
                                    csv_writer.writerow([wav_filename, wav_filesize, transcript])
                                    print(f"File {wav_filename} written")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python script_name.py /path/to/your/directory file_name.csv")
        sys.exit(1)


    root = sys.argv[1]                      # Change this to your root directory path
    outputFile = sys.argv[2]                # Output CSV file name
    ProcessFile(root, outputFile)