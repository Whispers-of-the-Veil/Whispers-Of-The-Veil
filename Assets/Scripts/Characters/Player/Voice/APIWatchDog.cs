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
            
            // Check if we are in the ed
            if (Application.isEditor) {
                path = Application.dataPath + "/Scripts/SpeechRecognition/Src/dist/";
            }
            else {
                path = System.Reflection.Assembly.GetEntryAssembly().Location;
            }

            // Check if the process is already running; if it is do nothing
            // if it isnt then 
            if (!IsProcessRunning("ASR_API")) {
                Debug.Log("Starting API");
                
                API = Process.Start(path + "ASR_API");
    
                if (API == null) {
                    Debug.Log("Failed to start API");
                }
            } else {
                Debug.Log("API is already running");
            }
        }
    }
}