using static Tensorflow.Binding;
using Tensorflow;
using Tensorflow.NumPy;
using System.Text;

namespace ASR {
    public class ASR {
        private Tensorflow.Keras.Engine.IModel model;
        private static SignalTransformer signal;
        private static Dictionary<int, char> numToChar = new Dictionary<int, char>();

        ASR(string modelPath) {
            model = tf.keras.models.load_model(modelPath);
            signal = new SignalTransformer();

            string vocab = "abcdefghijklmnopqrstuvwxyz";
            var characters = new List<char>(vocab);
            characters.Add(' ');

            for (int i = 0; i < characters.Count; i++) {
                numToChar[i] = characters[i];
            }
        }

        /// <summary>
        /// This method will convert the Tensor[] of the decoded softmax label into its string representation.
        /// </summary>
        /// <param name = "softmaxLabel">This is the softmax label output fo tt he Decode method</param>
        /// <returns>A string of the prediction</returns>
        private static string ConvertToString(Tensor[] softmaxLabel) {
            StringBuilder decodedString = new StringBuilder();

            foreach (var tensor in softmaxLabel) {
                int[] tensorValues = tensor.ToArray<int>();

                foreach (int value in tensorValues) {
                    if (numToChar.TryGetValue(value, out char character)) {
                        decodedString.Append(character);
                    }
                }
            }

            return decodedString.ToString();
        }

        /// <summary>
        /// This method will preform ctc_gready_decoder to conver the logits into a softmax
        /// prediction
        /// </summary>
        /// <param name = "logits">This the logits output from the model</param>
        /// <returns>Returns a Tensor[] of the softmax label</returns>
        private static Tensor[] Decode(Tensor logits) {
            var prediction = tf.nn.ctc_greedy_decoder(
                                            logits,
                                            np.ones((int)logits.shape[0]) * logits.shape[1]);

            return prediction;
        }

        /// <summary>
        /// This method will convert the array of audio samples into a spectrogram
        /// </summary>
        /// <param name = "audio">float[] of the audio samples</param>
        /// <returns>Returns an NDArray of the Spectrogram data</returns>
        private static NDArray Spectrogram(float[] audio) {
            Tensor spectrogram;
            Tensor mean;
            Tensor std;

            spectrogram = signal.STFT(audio, 256, 256, 384);
            spectrogram = tf.abs(spectrogram);
            spectrogram = tf.pow<Tensor, Tensor>(spectrogram, spectrogram);

            mean = tf.reduce_mean(spectrogram, 1, true);
            std = tf.reduce_std(spectrogram, 1, true);
            spectrogram = (spectrogram - mean) / (std + 1e-10);

            return np.expand_dims(spectrogram.numpy(), 0);
        }

        /// <summary>
        /// This method will abstract the process of converting the audio samples
        /// into their pridicted transcripts. It will process the float[] into spectrograms
        /// then get and decode the prediction from the model into a string var.
        /// </summary>
        /// <param name = "audio">float[] of the audio samples</param>
        /// <returns>Returns the string prediction from the model</returns>
        public string Predict(float[] audio) {
            var spectrogram = Spectrogram(audio);
            var logits = model.predict(spectrogram);
            var softmaxLabel = Decode(logits);
            var prediction = ConvertToString(softmaxLabel);

            return prediction;
        }
    }
}

