using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Characters.Player.Voice {
    [System.Serializable]
    public class ParseJson {
        public string prediction;
    }

    public class RecordAudio : MonoBehaviour {
        [Header("API Settings")]
        [SerializeField] string URL;

        [Header("Expected Keys")]
        [SerializeField] string[] keys;

        [Header("Recording Options")]
        [SerializeField] int SampleRate = 16000;
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
            if (Input.GetKeyDown(KeyCode.T)) {
                StartRecording();
            }

            if (Input.GetKeyUp(KeyCode.T)) {
                StopRecording();

                StartCoroutine(GetPrediction(audio));
            }
        }

        private void StartRecording() {
            if (!isRecording) {
                Debug.Log("Started Recording...");

                micClip = Microphone.Start(microphoneDevice, false, 10, SampleRate);
                isRecording = true;
            }
        }

        private void StopRecording() {
            if(isRecording) {
                Debug.Log("Stoped Recording");

                int samples = Microphone.GetPosition(microphoneDevice);

                Microphone.End(microphoneDevice);
                isRecording = false;

                audio = new float[samples];
                micClip.GetData(audio, 0);
            }
        }

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

                Debug.Log("You said: " + prediction);
            } else {
                Debug.LogError("Failed to send audio: " + www.error);
            }
        }
    }
}
