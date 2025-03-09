using System;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Characters.Player.Voice {
    public class APIWatchDog : MonoBehaviour {
        private Process API;
        
        void Start() {
            StartAPI();
        }

        private void Update() {
            WatchAPI();
        }
        
        /// <summary>
        /// Check if the process is running
        /// </summary>
        /// <returns>True if the process exits; false otherwise</returns>
        bool IsProcessRunning(string processName) {
            return Process.GetProcessesByName(processName).Length > 0;
        }
        
        /// <summary>
        /// This will check if the process has stopped running. If it does, attempt to restart it
        /// </summary>
        void WatchAPI() {
            if (!IsProcessRunning("ASR_API")) {
                Debug.Log("API stopped running; attempting to restart.");
                StartAPI();
            }
        }

        /// <summary>
        /// Attempt to start the API
        /// </summary>
        void StartAPI() {
            string path;
            
            // Get the path of the executable, both for the editor and build
            if (Application.isEditor) {
                // Poits inside the assets folder
                path = Application.dataPath + "/Scripts/SpeechRecognition/Src/dist/";
            }
            else {
                // Points inside the data folder (Whispers-of-the-Veil_Data)
                path = Application.dataPath + "/";
            }

            // Check if the process is already running; if it is, do nothing
            // if it isn't then 
            if (!IsProcessRunning("ASR_API")) {
                Debug.Log("Starting API");
                
                API = Process.Start(path + "ASR_API");
    
                if (API == null) {
                    Debug.LogError("Failed to start API");
                }
                
            } else {
                Debug.Log("API is already running");
            }
        }
    }
}