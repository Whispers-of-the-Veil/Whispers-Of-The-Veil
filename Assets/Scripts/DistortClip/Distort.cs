// Lucas Davis

using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using SpeechRecognition.Transform;

namespace DistortClip {
    public class Distort : MonoBehaviour {
        private SignalTransformer signalTransformer;
        private int channels;
        private int samples;
        
        private float[] ConvertToArray(AudioClip clip) {
            float[] audioData;
            
            this.channels = clip.channels;
            this.samples = clip.samples;
            
            audioData = new float[this.samples * this.channels];
            clip.GetData(audioData, 0);

            return audioData;
        }

        private AudioClip ConvertToClip(float[] audioData) {
            AudioClip newAudio = AudioClip.Create("DistoredVoiceClip", audioData.Length, this.channels, samples, false);
            newAudio.SetData(audioData, 0);

            return newAudio;
        }
        
        public AudioClip Clip(AudioClip clip) {
            if (clip == null) {
                Debug.Log("Null clip");
                return null;
            }

            float[] audioData = ConvertToArray(clip);
            this.signalTransformer = new SignalTransformer();

            Debug.Log("Original Length: " + audioData.Length);
            
            // Call the stft method with 25% overlap between each segment of length 512
            var freqValues = this.signalTransformer.stft(audioData, 512, 128);
            
            // Return the original clip if the STFT or FFT methods ran into a problem
            if (freqValues == null) {
                return clip;
            }
            
            // Call the stft method with 25% overlap between each segment of length 512
            float[] testAudioData = this.signalTransformer.stft(freqValues, 512, 128);
            
            // Return the original clip if the STFT or FFT methods ran into a problem
            if (testAudioData == null) {
                return clip;
            }
            
            Debug.Log("Length of inverse: " + testAudioData.Length);
            
            return ConvertToClip(testAudioData);
        }
    }
}