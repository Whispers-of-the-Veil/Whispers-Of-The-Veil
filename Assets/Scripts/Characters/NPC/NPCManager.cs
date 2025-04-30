using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using Management;

namespace Characters.NPC {
    public class NPCManager : MonoBehaviour {
        public static NPCManager instance;
        public List<NPCInfo> npcs = new List<NPCInfo>();
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        Blackboard blackboard;
        BlackboardKey followKey, moveKey, stayKey;
        
        public static event Action ResetBehavior;
        
        void OnEnable() => SceneManager.activeSceneChanged += OnSceneLoaded;
        void OnDisable() => SceneManager.activeSceneChanged -= OnSceneLoaded;
        private void OnSceneLoaded(Scene previousScene, Scene newScene) => StartCoroutine(HandleSceneLoaded());
        
        public DontDestroyManager dontDestroyManager {
            get => DontDestroyManager.instance;
        }

        void Start() {
            if (instance == null) {
                instance = this;
                dontDestroyManager.Track(this.gameObject);
            }
            else {
                Destroy(gameObject);
            }
            
            blackboard = controller.GetBlackboard();
            
            followKey = blackboard.GetOrRegisterKey("FollowCommand");
            moveKey = blackboard.GetOrRegisterKey("MoveCommand");
            stayKey = blackboard.GetOrRegisterKey("StayCommand");
        }

        public void Register(NPCInfo npc) => npcs.Add(npc);
        public void Unregister(int index) => npcs.RemoveAt(index);
        public int GetIndex(string id) => npcs.FindIndex(n => n.id == id);
        
        private IEnumerator HandleSceneLoaded() {
            yield return new WaitForSeconds(1f);
            
            ResetBehavior?.Invoke();
            
            foreach (var npc in npcs) {
                if (blackboard.TryGetValue(followKey, out bool follow) && follow) {
                    ClearAgentsPath(npc);
    
                    // Disable the agent to set its position manually
                    npc.agent.enabled = false;
                    npc.reference.transform.position = GameObject.Find("Player").transform.position;
                    npc.agent.enabled = true;
                }
                else {
                    if (npc.savedScene != SceneManager.GetActiveScene().name) {
                        ClearAgentsPath(npc);
                        
                        npc.agent.enabled = false;
                        npc.reference.SetActive(false);
                    }
                    else {
                        npc.reference.SetActive(true);
                        npc.reference.transform.position = npc.savedPosition;
                        npc.agent.enabled = true;
                        
                        npc.agent.SetDestination(npc.savedPosition);
                    }
                }
            }
        }

        void ClearAgentsPath(NPCInfo npc) {
            npc.agent.ResetPath();
            npc.agent.velocity = Vector3.zero;
        }
        
        private void OnDestroy() {
            if (instance == this) instance = null;
        }
    }
}