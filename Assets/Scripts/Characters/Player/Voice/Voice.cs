using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using System.Globalization;
using TMPro;
using Audio.SFX;
using Characters.Player.Sound;
using Unity.VisualScripting;
using Config;
using UnityEngine.Events;

namespace Characters.Player.Voice {
    public class Voice : MonoBehaviour {
        // API settings
        private API api {
            get => API.instance;
        }
        
        [Header("Audio")]
        [SerializeField] AudioClip correctClip;
        [SerializeField] AudioClip wrongClip;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }

        [Header("Entities")]
        [SerializeField] public GameObject[] enemies; // An array of enemy gameobjects that can hear the player voice
        [SerializeField] float DetectVoiceThreshold = 0.02f;

        [Header("Expected Keys")]
        [SerializeField] string[] keys;
        [SerializeField] int threshold;
        [SerializeField] int PuzzleInRange = 3;
        public static event Action<string> OnCommandRecognized;
        private int index;

        [Header("Recording Options")]
        [SerializeField] int SampleRate = 16000;
        [SerializeField] int RecordingLength = 5;
        private AudioClip micClip;
        private float[] audio;
        private Queue<float[]> recordings = new Queue<float[]>();
        private bool isRecording = false;
        private bool isProcessing = false;
        private string microphoneDevice;
        private bool canRecord = true;

        [Header("Speech Bubble")] 
        [HideInInspector] public bool spoke;
        private GameObject speechBubble;
        private TextMeshProUGUI textField;
        
        void Start () {
            speechBubble = GameObject.Find("SpeechBubble");
            textField = GameObject.Find("SpeechBubble/Text").GetComponent<TextMeshProUGUI>();

            speechBubble.SetActive(false);

            try {
                microphoneDevice = Microphone.devices[0];
            } catch (IndexOutOfRangeException) {
                Debug.LogError("No microphone detected!");

                canRecord = false;
            }

        }

        void Update () {
            if (!isRecording && canRecord) {
                StartCoroutine(HandleRecording());
            }

            if (recordings.Count > 0 && !isProcessing) {
                isProcessing = true;

                float[] clip = recordings.Dequeue();

                Debug.Log("Processing item in queue: " + recordings.Count + " items remaining");
                StartCoroutine(GetPrediction(clip));
            }
        }

        private IEnumerator HandleRecording() {
            Debug.Log("Started Recording...");
            isRecording = true;

            // Get a recording from the users microphone
            micClip = Microphone.Start(microphoneDevice, false, 10, SampleRate);

            yield return new WaitForSeconds(RecordingLength);

            int samples = Microphone.GetPosition(microphoneDevice);
            Microphone.End(microphoneDevice);

            // Set the recording into a micClip
            audio = new float[samples];
            micClip.GetData(audio, 0);

            // Determine if the player is speaking
            float sum = 0f;
            foreach (var sample in audio) {
                sum += sample * sample;
            }
            float rmsValue = Mathf.Sqrt(sum / samples);

            Debug.Log(rmsValue);

            if (rmsValue > DetectVoiceThreshold) {
                recordings.Enqueue(audio);
                SoundManager.ReportSound(transform.position);
            }

            Debug.Log("Stoped Recording");
            isRecording = false;
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
        /// Determines if the player is within range of a puzzle object -- defined by the puzzle layer.
        /// </summary>
        /// <returns>Returns true if they are; false otherwise</returns>
        private bool CheckIfNearPuzzle() {
            Collider2D[] puzzleColliders = Physics2D.OverlapCircleAll(transform.position, PuzzleInRange, LayerMask.GetMask("Puzzle"));

            // Loop through all colliders and check line of sight
            foreach (Collider2D puzzleCollider in puzzleColliders) {
                Vector2 directionToPuzzle = (puzzleCollider.transform.position - transform.position).normalized;

                // Perform a 2D raycast to check for line of sight to the puzzle
                if (Physics2D.Raycast(transform.position, directionToPuzzle, PuzzleInRange, LayerMask.GetMask("Puzzle"))) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the prediction from the model API
        /// </summary>
        /// <param name="audioData">The audio data collected from the microphone</param>
        private IEnumerator GetPrediction(float[] audioData) {
            string prediction = " ";
            byte[] audioBytes = new byte[audioData.Length * sizeof(float)];
            System.Buffer.BlockCopy(audioData, 0, audioBytes, 0, audioBytes.Length);
            
            // Timeout / running doesn't set if we start the scene outside of the main menu screen.
            if (APIWatchDog.Running && !APIWatchDog.Timeout) {
                yield return api.SendWebRequest(
                    "ASR",
                    audioBytes,
                    (response) => {
                        if (!String.IsNullOrEmpty(response)) {
                            prediction = response;
                        }
                    },
                    (error) => {
                        Debug.Log("Failed to send audio: " + error);
    
                        prediction = "Ugh... my head feels fuzzy...";
                    }
                );
            }

            Debug.Log("You said: " + prediction);
            textField.text = prediction;
            
            // Check if we are near a puzzle. if we are, check the prediction agains the
            // keys array
            if (CheckIfNearPuzzle()) {
                speechBubble.SetActive(true);
                
                Debug.Log("Near puzzle");
                yield return StartCoroutine(CheckExpected(prediction));

                try {
                    if (index == -1) {
                        Debug.Log("You said something the game wasnt expecting");
                        textField.text = "hmm, that was weird... I should try that again.";

                        sfxManager.PlaySFX(wrongClip, transform, 1f);
                    }
                    else if (index < keys.Length) {
                        Debug.Log("The closest expected string is: " + keys[index]);
                        textField.text = keys[index];

                        sfxManager.PlaySFX(correctClip, transform, 1f);

                        OnCommandRecognized?.Invoke(keys[index]);
                    }
                    else {
                        throw new Exception("Index fell outside the bounds of the key array");
                    }
                }
                catch (Exception e) {
                    Debug.Log(e.Message);
                }
                finally {
                    StartCoroutine(ResetSpeechBubble(3));
                }
            }

            isProcessing = false;
        }

        /// <summary>
        /// Resets the speech bubble after a specified amount of time
        /// </summary>
        /// <param name="time">The amount of time to wait before disabling the speechbubble</param>
        /// <returns></returns>
        private IEnumerator ResetSpeechBubble(int time) {
            yield return new WaitForSeconds(time);
            
            textField.text = "...";
            speechBubble.SetActive(false);
        }

        /// <summary>
        /// When the game is closed, send a web request for the API to shut down
        /// </summary>
        void OnApplicationQuit() {
            api.ShutDown();
        }
    }
}