# Speech Recognition
## Introduction
This library allows you to train your own Speech Recognition model, and utilize it in Unity.

## Required Libraries
The following are the required libraries you need to install to utilize the training python scripts, and to integrate the model into untiy.

Python scripts require:
<ul>
    <li>tensorflow</li>
    <li>numpy</li>
    <li>pandas</li>
    <li>matplotlib</li>
    <li>jiwer</li>
    <li>os</li>
    <li>Sys</li>
    <li>csv</li>
    <li>re</li>
</ul>

We will be utilizing the Barracuda package to interact with the model in unity.

## Model Architecture
This model utilizes The Deepspeech 2 actitecture
    https://nvidia.github.io/OpenSeq2Seq/html/speech-recognition/deepspeech2.html
    
    https://github.com/NVIDIA/OpenSeq2Seq/blob/master/example_configs/speech2text/ds2_small_1gpu.py

Located in the SpeecRecogntion/Training directory is the License for the Deepspeech 2 model.

![Model Architecture](Diagrams/ModelArchitecture.png)

## Training
Before we start the training process, we need to make sure to have three csv files ready; Training, Validation, and 
Testing. These files should have the following structure.

    filename,transcript,
    /complete/path/to/wav/file.wav,Transcript of wav file

Please note: all wav files used in the Training, Validation, and Testing datasets should have the same sample rates. In
our case, that would be 16000 samples.

Certain parameters can be modified within the 'config.ini' file located at the root of the Training directory. For 
example, we can enable or disable data augmentation for the Training dataset. The augmentation includes Additive noise, 
time streching, and volume modulation.

For our model, we used the Librispeech dataset with the following setup:
*Round 1*: We utilized the 100 dataset for our Training data, the combined Dev-clean and Dev-other datasets for our 
Validation data, and the combined Test-clean and Test-other for our Test data. We had the following values for data 
augmentation of the 100 dataset: *noise_min and noise_max were set to 30 and 20, time_strech_ratio was set to 5, and 
Volume was set to 0.*

*Round 2*: We utilized the 360 Dataset for our Training data. The Validation and Testing Dataset remained the same. We had 
the following values for data augmentation of the 360 dataset: *noise_min and noise_max were set to 45 and 20, 
time_strech_ratio was set to 10, Volume was set to 5.*

*Round 3*: We utilized the 500 Dataset for our Training data *without any augmentation.* The validation and Testing Datasets 
remained the same.

Once the csv files and the config.ini file are ready, run the following cmd to start the training process (Please be sure
to center on the Training directory):

    python -m Model.TrainASRModel TrainingDataset.csv ValidationDataset.csv TestDataset.csv

### Converting Model
After training has completed, you will be left with a model saved with the .keras extension. In order to use this model
outside of python we will need to run a few scripts.<br>
First we will need to run the ConvertToPB.py script using

    python -m scripts.ConvertToPB.py /path/to/model.keras /path/to/a/folder

This script will output what the input and output nodes are; hold onto them, we will use them in the next cmd. Once
everything has been converted to the protocol buffer format, we can then convert it to a format that unity expects
using the tf2onnx module.

    python -m tf2onnx.convert --input /path/to/model.pb --inputs <input node> --outputs <output node> --output /path/to/Model.onnx

This will produce a model.onnx file, of which we can use in unity.

### Diagrams


## Usage

### Diagrams
