// Lucas

using System.Collections.Generic;
using UnityEngine;

namespace Management {
    public class DontDestroyManager : MonoBehaviour {
        public static DontDestroyManager instance;
        private static List<GameObject> tracked = new();
    
        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else {
                Destroy(gameObject);
            }
        }
    
        public void Track(GameObject go) {
            tracked.Add(go);
            DontDestroyOnLoad(go);
        }
    
        public void ResetAll() {
            foreach (var go in tracked) {
                if (go != null) Destroy(go);
            }
            tracked.Clear();
        }
    }
}
