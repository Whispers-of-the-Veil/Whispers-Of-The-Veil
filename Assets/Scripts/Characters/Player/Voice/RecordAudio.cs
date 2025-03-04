using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using System.Globalization;
using TMPro;

namespace Characters.Player.Voice {
    [System.Serializable]
    public class ParseJson {
        public string prediction;
    }

    public class RecordAudio : MonoBehaviour {
        public static event Action<string> OnCommandRecognized;
        
        [Header("API Settings")]
        [SerializeField] string URL;

        [Header("Expected Keys")]
        [SerializeField] string[] keys;
        [SerializeField] int threshold;
        private int index;

        [Header("Recording Options")]
        [SerializeField] int SampleRate = 16000;
        [SerializeField] int TimeLimit = 5;
        private AudioClip micClip;
        private float[] audio;
        private bool isRecording;
        private string microphoneDevice;

        void Start () {
            isRecording = false;

            if (Microphone.devices.Length > 0) {
                microphoneDevice = Microphone.devices[0];
            } else {
                Debug.LogError("No microphone detected!");
            }
        }

        void Update () {
            
            if (Input.GetKeyDown(KeyCode.T) && !isRecording) {
                StartRecording();
                StartCoroutine(RecordingLimit());
            }

            if (Input.GetKeyUp(KeyCode.T) && isRecording) {
                StopRecording();

                StartCoroutine(GetPrediction(audio));
            }
        }

        private void StartRecording() {
            Debug.Log("Started Recording...");

            micClip = Microphone.Start(microphoneDevice, false, 10, SampleRate);
            isRecording = true;
        }

        private void StopRecording() {
            Debug.Log("Stoped Recording");

            int samples = Microphone.GetPosition(microphoneDevice);

            Microphone.End(microphoneDevice);
            isRecording = false;

            audio = new float[samples];
            micClip.GetData(audio, 0);
        }
        
        /// <summary>
        /// Calculate the difference between 2 strings using the Levenshtein distance algorithm
        /// </summary>
        /// <param name="pred">The prediction from the model</param>
        /// <param name="expected">One of the expected keys from the keys array</param>
        /// <returns>The distance between the two strings</returns>
        private int LevenshteinDistance(string pred, string expected) {
            var predLength = pred.Length;
            var expectedLength = expected.Length;

            var matrix = new int[predLength + 1, expectedLength + 1];

            if (predLength == 0)
                return expectedLength;

            if (expectedLength == 0)
                return predLength;

            for (var i = 0; i <= predLength; matrix[i, 0] = i++){}
            for (var j = 0; j <= expectedLength; matrix[0, j] = j++){}

            for (var i = 1; i <= predLength; i++) {
                for (var j = 1; j <= expectedLength; j++) {
                    var cost = (expected[j - 1] == pred[i - 1]) ? 0 : 1;

                    matrix[i, j] = Mathf.Min(
                        Mathf.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            
            return matrix[predLength, expectedLength];
        }

        /// <summary>
        /// Check to see if the prediction is similar to any of the excepted keys
        /// </summary>
        /// <param name="prediction">The prediction generated from the model</param>
        private IEnumerator CheckExpected(string prediction) {
            int closest = 0;
            int distance;
            index = -1;
            
            for (int i = 0; i < keys.Length; i++) {
                distance = LevenshteinDistance(prediction, keys[i]);

                Debug.Log(distance);

                if (distance < threshold) {
                    if (closest == 0) {
                        closest = distance;
                        index = i;
                    } else if (distance < closest) {
                        closest = distance;
                        index = i;
                    }
                }
            }

            yield return null;
        }

        /// <summary>
        /// Get the prediction from the model API
        /// </summary>
        /// <param name="audioData">The audio data collected from the microphone</param>
        private IEnumerator GetPrediction(float[] audioData) {
            byte[] audioBytes = new byte[audioData.Length * sizeof(float)];
            System.Buffer.BlockCopy(audioData, 0, audioBytes, 0, audioBytes.Length);

            UnityWebRequest www = UnityWebRequest.PostWwwForm(URL, "");
            www.uploadHandler = new UploadHandlerRaw(audioBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) {
                var json = www.downloadHandler.text;

                ParseJson response = JsonUtility.FromJson<ParseJson>(json);

                string prediction = response.prediction;

                if (prediction == null || prediction.Length == 0) {
                    prediction = " ";
                }

                Debug.Log("You said: " + prediction);
                
                yield return StartCoroutine(CheckExpected(prediction));
                
                if (index == -1) {
                    Debug.Log("You said something the game wasnt expecting");
                } else if (index < keys.Length) {
                    Debug.Log("The closest expected string is: " + keys[index]);
                    OnCommandRecognized?.Invoke(keys[index]);
                } else {
                    Debug.Log("Index fell outside the bounds of the keys array");
                }
            } else {
                Debug.LogError("Failed to send audio: " + www.error);
            }
        }

        /// <summary>
        /// This function will stop the reocrding if it goas about the time limit
        /// </summary>
        /// <returns></returns>
        private IEnumerator RecordingLimit() {
            yield return new WaitForSeconds(TimeLimit);

            if (isRecording) {
                StopRecording();

                StartCoroutine(GetPrediction(audio));
            }
        }
    }
}