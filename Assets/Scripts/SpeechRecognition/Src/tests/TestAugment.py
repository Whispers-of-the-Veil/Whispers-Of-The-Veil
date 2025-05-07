# Lucas

import tensorflow as tf

import unittest
from unittest.mock import Mock

from Data.Augment import Augment

class TestAugment(unittest.TestCase):
    def test_noise(self):
        augment = Augment()

        mock_uniform = Mock()
        mock_uniform.side_effect = [tf.constant(-1.5), tf.constant([0.1, 0.2, 0.3])]

        tf.random.uniform = mock_uniform

        _audio = tf.constant([0.5, 0.5, 0.5], dtype=tf.float32)

        result = augment.Noise(_audio)

        expected_noise = tf.constant([0.1, 0.2, 0.3], dtype=tf.float32) * tf.math.pow(10.0, -1.5 / 20.0)
        expected_result = _audio + expected_noise

        self.assertTrue(tf.reduce_all(tf.equal(result, expected_result)))

    def test_volume(self):
        augment = Augment()

        mock_uniform = Mock(return_value=tf.constant(1.05))
        tf.random.uniform = mock_uniform

        _audio = tf.constant([0.5, 0.5, 0.5], dtype=tf.float32)

        result = augment.Volume(_audio)

        expected_result = _audio * tf.constant(1.05)

        print("Result:", result.numpy())
        print("Expected Result:", expected_result.numpy())

        self.assertTrue(tf.reduce_all(tf.equal(result, expected_result)))
        mock_uniform.assert_called_once_with(shape=[], minval=0.95, maxval=1.05)

def main():
    unittest.main(verbosity = 2)

if __name__ == '__main__':
    main()