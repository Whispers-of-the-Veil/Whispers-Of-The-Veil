using UnityEngine;
using UnityEngine.SceneManagement;
using Characters.Player;

namespace Dialogue
{
    public class HelperHintController : MonoBehaviour
    {
        public static HelperHintController Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private Animator helperAnimator;

        public DialogueObject CurrentHint { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (player == null)
                player = FindObjectOfType<PlayerController>();

            if (helperAnimator == null)
            {
                // Try to find the "Echo" child GameObject directly under the Player
                Transform echoTransform = player.transform.Find("Echo");
        
                if (echoTransform != null)
                {
                    helperAnimator = echoTransform.GetComponent<Animator>();
                    Debug.Log("Helper Echo found and Animator is assigned.");
                }
                else
                {
                    Debug.LogError("Echo GameObject not found under Player!");
                }
            }
        }



        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H) && CurrentHint != null)
            {
                ShowHint();
            }
        }

        public void SetCurrentHint(DialogueObject hint)
        {
            CurrentHint = hint;
        }

        public void ClearCurrentHint()
        {
            CurrentHint = null;
        }

        private void ShowHint()
        {
            if (helperAnimator != null)
            {
                // ✅ Ensure the GameObject is enabled first
                helperAnimator.gameObject.SetActive(true);

                // ✅ Then trigger the animation
                helperAnimator.SetTrigger("Show");
            }

            if (player != null && CurrentHint != null)
            {
                player.DialogueUI.ShowDialogue(CurrentHint);
                player.DialogueUI.OnDialogueFinished += HideHelper;
            }
        }




        private void HideHelper()
        {
            if (helperAnimator != null)
            {
                // Trigger the hide animation (if you want an animation for hiding)
                helperAnimator.SetTrigger("Hide");
        
                // Ensure the helper object is set to inactive
                GameObject helperObject = helperAnimator.gameObject;
                helperObject.SetActive(false);
            }

            // Unsubscribe from the dialogue finish event to prevent multiple triggers
            if (player != null && player.DialogueUI != null)
            {
                player.DialogueUI.OnDialogueFinished -= HideHelper;
            }
        }


    }
}
