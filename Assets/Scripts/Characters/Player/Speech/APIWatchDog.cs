// Lucas

using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Characters.Player.Speech {
    public class APIWatchDog : MonoBehaviour {
        public static bool Running = false;
        public static bool Timeout = false;
        
        private Process apiProcess;
        
        private API api {
            get => API.instance;
        }
        
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
        public static bool IsProcessRunning(string processName) {
            return Process.GetProcessesByName(processName).Length != 0;
        }
        
        /// <summary>
        /// This will check if the process has stopped running. If it does, attempt to restart it
        /// </summary>
        private void WatchAPI() {
            if (!IsProcessRunning("ASR_API")) {
                Running = false;
                Debug.Log("API stopped running; attempting to restart.");
                StartAPI();
            }
            else {
                Running = true;
            }
        }

        /// <summary>
        /// Attempt to start the API
        /// </summary>
        private void StartAPI() {
            string path;
            
            // Get the path of the executable, both for the editor and build
            if (Application.isEditor) {
                // Poits inside the assets folder
                path = Application.dataPath + "/Scripts/SpeechRecognition/Src/dist/";
            }
            else {
                // For Windows and Linux machines, this points to the Data folder
                // For MacOS, this points to the contents folder within the app package
                path = Application.dataPath + "/";
            }

            // Check if the process is already running; if it is, do nothing
            // if it isn't then 
            if (!IsProcessRunning("ASR_API")) {
                Debug.Log("Starting API");
                
                apiProcess = Process.Start(path + "ASR_API");
    
                if (apiProcess == null) {
                    Debug.LogError("Failed to start API");
                }
                
            } else {
                Debug.Log("API is already running");
            }
        }
    }
}