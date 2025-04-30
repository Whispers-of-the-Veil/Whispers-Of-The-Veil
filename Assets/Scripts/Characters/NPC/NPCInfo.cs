using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Characters.NPC {
    public struct NPCInfo {
        public string id;
        public GameObject reference;
        public NavMeshAgent agent;
        public string savedScene;
        public Vector2 savedPosition;
    };
}