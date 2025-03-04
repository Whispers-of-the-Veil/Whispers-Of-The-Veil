import tensorflow as tf

from Data.Process import Process
from Data.Augment import Augment
from Data.Validate import Validate

from Grab_Ini import ini

class Setup:
    def __init__(self, process: Process, augment: Augment, validate: Validate):
        self.process   = process
        self.augment   = augment
        self.validate  = validate

        generalConfig  = ini().grabInfo("config.ini", "General")
        trainingConfig = ini().grabInfo("config.ini", "Training")
        processConfig  = ini().grabInfo("config.ini", "Process")

        self.augmentData = eval(processConfig['augment'])
        self.batchSize    = int(trainingConfig['batch_size'])
        seed         = int(generalConfig['seed'])

        tf.random.set_seed(seed)

    def CreateDataset(self, _training, _validation, _test):
        """
        Creates a dataset for both the training and validatation sets. It will map the audio paths and transcripts
        to the process.Data method to process them into spectrograms and labels.

        Parameters:
            - _training: The path to the training data csv file
            - _validation: The path to the validation data csv file

        Returns:
            Two tensorflow datasets containing the training and validation sets
        """
        trainAudioPaths, trainTranscripts = self.process.LoadCSV(_training)
        validAudioPaths, validTranscripts = self.process.LoadCSV(_validation)
        testAudioSamples, testTranscripts = self.process.LoadCSV(_test)

        # The train dataset calls a different method that will augment the audio samples
        trainDataset = tf.data.Dataset.from_tensor_slices(
            (list(trainAudioPaths), list(trainTranscripts))
        )
        trainDataset = (
            trainDataset.map(self.ProcessTrainingData, num_parallel_calls = tf.data.AUTOTUNE)
            .padded_batch(self.batchSize)
            .prefetch(buffer_size = tf.data.AUTOTUNE)
        )

        # The validation and testing datasets arent augmented. This is to keep them the same
        # between different training sets so we can get an accurate val_loss and Error_Rate
        # measurement
        validationData = tf.data.Dataset.from_tensor_slices(
            (list(validAudioPaths), list(validTranscripts))
        )
        validationData = (
            validationData.map(self.ProcessData, num_parallel_calls = tf.data.AUTOTUNE)
            .padded_batch(self.batchSize)
            .prefetch(buffer_size = tf.data.AUTOTUNE)
        )

        testDataset = tf.data.Dataset.from_tensor_slices(
            (list(testAudioSamples), list(testTranscripts))
        )
        testDataset = (
            testDataset.map(self.ProcessData, num_parallel_calls = tf.data.AUTOTUNE)
            .padded_batch(self.batchSize)
            .prefetch(buffer_size = tf.data.AUTOTUNE)
        )

        return trainDataset, validationData, testDataset
    
    def Debug(self, _train, _valid, _test):
        """
        This function will display the labels and spcetrograms using process' ValidateData method.
        This is used to debug the spectrograms and the labels if there is a problem

        Parameters:
            - process: A reference to the process class
            - _train: The training dataset
            - _valid: The validation dataset
            - _test: The testing dataset
        """
        for spectrogram, label in _train.take(1):
            self.validate.Spectrogram(spectrogram[0], label[0])

        for spectrogram, label in _valid.take(1):
            self.validate.Spectrogram(spectrogram[0], label[0])

        for spectrogram, label in _test.take(1):
            self.validate.Spectrogram(spectrogram[0], label[0])

    def ProcessData(self, _file, _transcript):
        """
        Process an audio file
        """
        audio = self.process.LoadAudioFile(_file)

        spec = self.process.Spectrogram(audio)
        spec = self.process.NormalizeSpec(spec)

        label = self.process.Transcript(_transcript)

        return spec, label    

    def ProcessTrainingData(self, _file, _transcript):
        audio = self.process.LoadAudioFile(_file)

        if (self.augmentData):
            audio = self.augment.Noise(audio)
            # audio = self.augment.TimeStrech(audio)
            audio = self.augment.Volume(audio)

        spec = self.process.Spectrogram(audio)
        spec = self.process.NormalizeSpec(spec)

        label = self.process.Transcript(_transcript)

        return spec, label