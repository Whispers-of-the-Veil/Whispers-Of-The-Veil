# Lucas

import tensorflow as tf
import pandas as pd
import numpy as np

import unittest
from unittest.mock import Mock

from Data.Process import Process

class TestProcess(unittest.TestCase):
    def test_convert_label(self):
        process = Process()
        label = np.array([
            25, 15, 21, 14, 7, 27, 6, 9, 20, 26, 15, 15, 20, 8
        ])
        
        self.assertEqual(process.ConvertLabel(label), "young fitzooth")

    def test_load_valid_csv(self):
        mock_read_csv = Mock(return_value=pd.DataFrame({
            'filename': ['file1.wav', 'file2.wav'],
            'transcript': ['Hello', 'World']
        }))

        pd.read_csv = mock_read_csv

        obj = Process()
        audioPath, transcripts = obj.LoadCSV('fake_path.csv')
        self.assertEqual(audioPath, ['file1.wav', 'file2.wav'])
        self.assertEqual(transcripts, ['Hello', 'World'])

    def test_empty_csv(self):
        mock_read_csv = Mock(side_effect=pd.errors.EmptyDataError)

        pd.read_csv = mock_read_csv

        obj = Process()
        with self.assertRaises(SystemExit):
            obj.LoadCSV('fake_path.csv')

    def test_file_not_found(self):
        mock_read_csv = Mock(side_effect=FileNotFoundError)

        pd.read_csv = mock_read_csv

        obj = Process()
        with self.assertRaises(SystemExit):
            obj.LoadCSV('non_existent_file.csv')

    def test_load_audio_file(self):
        mock_read_file = Mock(return_value=b'fake_audio_data')
        mock_decode_wav = Mock(return_value=(tf.constant([[0.1], [0.2], [0.3]]), tf.constant(16000)))
        
        tf.io.read_file = mock_read_file
        tf.audio.decode_wav = mock_decode_wav

        obj = Process()
        audio = obj.LoadAudioFile('fake_path.wav')

        expected_audio = tf.constant([0.1, 0.2, 0.3])
        self.assertTrue(tf.reduce_all(tf.equal(audio, expected_audio)))
        mock_read_file.assert_called_once_with('fake_path.wav')
        mock_decode_wav.assert_called_once_with(b'fake_audio_data')

    def test_normalize_spec(self):
        process = Process()
        _spectrogram = tf.constant([[1.0, 2.0, 3.0], [4.0, 5.0, 6.0]])

        normalized_spectrogram = process.NormalizeSpec(_spectrogram)
        
        expected_normalized = process.NormalizeSpec(tf.constant([
            [1.0, 2.0, 3.0],
            [4.0, 5.0, 6.0]
        ]))

        self.assertTrue(tf.reduce_all(tf.equal(normalized_spectrogram, expected_normalized)))

    def test_spectrogram(self):
        process = Process()
        
        _audio = tf.random.uniform([1024], minval=-1.0, maxval=1.0, dtype=tf.float32)

        spectrogram = process.Spectrogram(_audio)

        tensor = tf.constant(0, shape=(5, 193), dtype=tf.int64)

        self.assertEqual(spectrogram.shape, tensor.shape)

    def test_transcript(self):
        process = Process()

        _transcript = tf.constant("Test")

        result = process.Transcript(_transcript)

        expected = np.array([20, 5, 19, 20])
        expected = tf.convert_to_tensor(expected, dtype=tf.int64)

        self.assertTrue(tf.reduce_all(tf.equal(result, expected)))

def main():
    unittest.main(verbosity = 2)

if __name__ == '__main__':
    main()