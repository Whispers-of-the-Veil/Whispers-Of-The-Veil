import pandas as pd
import re
import sys

CSVPATH = "/home/ldavis/Code/Data_Sets/SpeechRec/Data/CSVs/lj.csv"

path = "/home/ldavis/Code/Data_Sets/SpeechRec/LJSpeech-1.1/wavs/"
extention = ".wav"

data = pd.read_csv(CSVPATH)

audioPath = data['filename'].tolist()
transcripts = data['transcript'].tolist()

updatedAudioPaths = []
updatedTranscripts = []

for item in audioPath:
    updatedAudioPaths.append(path + item + extention)

for item in transcripts:
    item = str(re.sub(r'[^a-zA-Z\s]', '', item))
    updatedTranscripts.append(item)

data = pd.DataFrame({
    'filename': updatedAudioPaths,
    'transcript': transcripts
})

data.to_csv(CSVPATH)