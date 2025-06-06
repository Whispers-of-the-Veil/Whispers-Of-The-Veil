// Lucas

using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using Audio.SFX;
using Characters.NPC;
using UnityEngine.UI;
using UnityEngine.UI;

namespace Characters.Player.Speech {
    public class Voice : MonoBehaviour {
        public bool useSpeechModel = true;
        [HideInInspector] public bool displayBubble;
        
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
        private TMP_InputField inputField;
        
        [Header("Progress Sprites")]
        [SerializeField] Sprite[] progressSprites;
        private Image image;
        private GameObject microphone;

        public SoundExpert soundExpert {
            get => SoundExpert.instance;
        }

        public SettingsManager settingsManager {
            get => SettingsManager.instance;
        }

        void Start () {
            speechBubble = GameObject.Find("SpeechBubble");
            textField = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            inputField = gameObject.GetComponentInChildren<TMP_InputField>();

            speechBubble.SetActive(false);

            try {
                microphoneDevice = settingsManager.selectedMicrophone;
            } catch (IndexOutOfRangeException) {
                Debug.LogError("No microphone detected!");

                canRecord = false;
            }

            microphone = transform.Find("Microphone").gameObject;
            image = transform.Find("Microphone/Image").GetComponent<Image>();
            
            microphone.SetActive(false);
        }

        void Update () {
            microphone.SetActive(isRecording);
            
            microphoneDevice = settingsManager.selectedMicrophone;
            
            if (!useSpeechModel) {
                textField.gameObject.SetActive(false);
                inputField.gameObject.SetActive(true);
            }
            else {
                textField.gameObject.SetActive(true);
                inputField.gameObject.SetActive(false);
            }
            
            if (useSpeechModel) {
                if (Input.GetKeyDown(KeyCode.BackQuote)) {
                    if (!isRecording && canRecord) {
                        StartCoroutine(HandleRecording());
                        StartCoroutine(AnimateProgress());
                    }
                }
                
                if (recordings.Count > 0 && !isProcessing) {
                    isProcessing = true;

                    float[] clip = recordings.Dequeue();

                    Debug.Log("Processing item in queue: " + recordings.Count + " items remaining");
                    StartCoroutine(GetPrediction(clip));
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.BackQuote)) displayBubble = !displayBubble;
                speechBubble.SetActive(displayBubble);

                if (displayBubble) {
                    Time.timeScale = 0f;
                    
                    if (Input.GetKeyDown(KeyCode.Return)) {
                        if (!isProcessing) {
                            StartCoroutine(CheckPrediction(inputField.text));
                            soundExpert.ReportSound(transform.position);
                        }
                    }
                }
                else {
                    Time.timeScale = 1f;
                }
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
                if (useSpeechModel) recordings.Enqueue(audio);
                soundExpert.ReportSound(transform.position);
            }

            Debug.Log("Stoped Recording");
            isRecording = false;
            
            image.sprite = progressSprites[0];
        }

        private IEnumerator AnimateProgress() {
            float timer = 0f;
            int totalFrames = progressSprites.Length;

            while (timer < RecordingLength) {
                float progress = timer / RecordingLength;

                int frameIndex = Mathf.FloorToInt(progress * (totalFrames - 1));
                
                image.sprite = progressSprites[frameIndex];
                
                yield return null;
                
                timer += Time.deltaTime;
            }
            
            image.sprite = progressSprites[totalFrames - 1];
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
            
            yield return CheckPrediction(prediction);
            
            StartCoroutine(ResetSpeechBubble(3));

            isProcessing = false;
        }

        private IEnumerator CheckPrediction(string prediction) {
            isProcessing = true;
            
            Debug.Log("You said: " + prediction);
            textField.text = prediction;
            
            speechBubble.SetActive(true);
            
            // Check if we are near a puzzle. if we are, check the prediction agains the
            // keys array
            if (CheckIfNearPuzzle()) {
                Debug.Log("Near puzzle");
                yield return StartCoroutine(CheckExpected(prediction));
                
                yield return new WaitForSeconds(1f);

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
            displayBubble = false;
        }

        /// <summary>
        /// When the game is closed, send a web request for the API to shut down
        /// </summary>
        void OnApplicationQuit() {
            api.ShutDown();
        }
    }
}