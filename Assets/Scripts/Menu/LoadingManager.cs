using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Characters.Player.Voice;
using TMPro;

public class LoadingManager : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private string nextScene;
    [SerializeField] private float timeout = 60f;
    private float timer = 0f;
    
    private float processProgress = 0f;
    private float apiProgress = 0f;
    private float sceneProgress = 0f;

    private bool isProcessRunning;
    private bool isConnected;
        
    private int dotCounter = 0;
    private int frames = 30;
    
    private API api {
        get => API.instance;
    }
    
    void Start() {
        StartCoroutine(LoadAsyncScene());
    }

    IEnumerator LoadAsyncScene() {
        statusText.text = "Checking API process...";
        while (!isProcessRunning) {
            if (!isProcessRunning) {
                isProcessRunning = APIWatchDog.IsProcessRunning("ASR_API");
                processProgress = isProcessRunning ? 0.25f : processProgress;
            }

            progressBar.fillAmount = processProgress + apiProgress + sceneProgress;

            yield return new WaitForSeconds(1);
        }
        
        statusText.text = " ";
        
        while (!isConnected) {
            statusText.text = "API - Testing connection" + new string('.', (dotCounter / frames) % 4);
            dotCounter++;
            
            timer += Time.deltaTime;
            
            if (timer > timeout) {
                statusText.text = "Connection Timeout!";
                APIWatchDog.Timeout = true;
                yield return new WaitForSeconds(1);
                break;
            }
            
            if (!isConnected) {
                yield return StartCoroutine(api.TestConnection((success) => {
                    isConnected = success;
                    APIWatchDog.Running = true;
                    apiProgress = success ? 0.25f : apiProgress;
                }));
            }
            
            progressBar.fillAmount = processProgress + apiProgress + sceneProgress;
            
            yield return new WaitForEndOfFrame();
        }

        if (APIWatchDog.Running) {
            statusText.text = "Connected Successfully!";
            yield return new WaitForSeconds(1);
        }
        
        AsyncOperation gameLevel = SceneManager.LoadSceneAsync(nextScene);
        
        while (!gameLevel.isDone) {
            statusText.text = "Loading scene" + new string('.', (dotCounter / frames) % 4);
            dotCounter++;
            
            sceneProgress = Mathf.Lerp(sceneProgress, 0.5f, 0.25f);
            progressBar.fillAmount = processProgress + apiProgress + sceneProgress;
            yield return new WaitForEndOfFrame();
        }

        progressBar.fillAmount = 1f;
    }
}
