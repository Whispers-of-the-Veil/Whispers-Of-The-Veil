// Lucas

using System;
using System.Collections;
using Config;
using UnityEngine;
using UnityEngine.Networking;

namespace Characters.Player.Speech {
    [System.Serializable]
    public class ParseJson {
        public string prediction;
    }
    
    public class API : MonoBehaviour {
        public static API instance;
        
        private Ini _ini;
        private string _url;

        protected void Awake() {
            if (instance == null) {
                _ini = new Ini("config.ini");
                this._url = GetURL();
                
                instance = this;
                DontDestroyOnLoad(this);
            }
            else {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Concatenate the values found in the ini file into an http site
        /// </summary>
        private string GetURL() {
            string ip = _ini.GetValue(Ini.Sections.API, Ini.Keys.address);
            string port = _ini.GetValue(Ini.Sections.API, Ini.Keys.port);
            
            return "http://" + ip + ":" + port + "/";
        }

        /// <summary>s
        /// Check if we are able to connect to the API
        /// </summary>
        /// <param name="onComplete">A lambda function: Defines the behavior we want after checking</param>
        public IEnumerator TestConnection(Action<bool> onComplete) {
            UnityWebRequest www = UnityWebRequest.Get(_url);

            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success) {
                onComplete?.Invoke(true);
            }
            else {
                onComplete?.Invoke(false);
            }
        }
        
        /// <summary>
        /// Send data to a specified endpoint to the API
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="data"></param>
        /// <param name="onSuccess">Lambda Function: defines the behavior for successfully posting the data</param>
        /// <param name="onError">Lambda function: defines the behavior for an unsuccessful post</param>
        public IEnumerator SendWebRequest(string endpoint, byte[] data, Action<string> onSuccess, Action<string> onError) {
            UnityWebRequest www = UnityWebRequest.PostWwwForm(this._url + endpoint, "");
            www.uploadHandler = new UploadHandlerRaw(data);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/octet-stream");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) {
                var json = www.downloadHandler.text;
                ParseJson response = JsonUtility.FromJson<ParseJson>(json);
                
                onSuccess?.Invoke(response.prediction);
            }
            else {
                onError?.Invoke(www.error);
            }
        }

        /// <summary>
        /// Shuts down the API Porcess; should be used when the application is closed
        /// </summary>
        public void ShutDown() {
            Debug.Log("Shutting Down API...");
            UnityWebRequest www = UnityWebRequest.Get(_url + "close");
            
            var request = www.SendWebRequest();

            while (!request.isDone) { }
        }
    }
}