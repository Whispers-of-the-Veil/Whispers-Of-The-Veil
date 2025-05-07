// Lucas

using Management;
using UnityEngine;
namespace Characters.NPC.BlackboardSystem.Control {
    public class BlackboardController : MonoBehaviour {
        public static BlackboardController instance;
        
        [SerializeField] BlackboardData blackboardData;
        readonly Blackboard blackboard = new Blackboard();
        readonly Arbiter arbiter = new Arbiter();
        
        public DontDestroyManager dontDestroyManager {
            get => DontDestroyManager.instance;
        }

        void Awake() {
            if (instance == null) {
                instance = this;
                dontDestroyManager.Track(this.gameObject);
            }
            else {
                Destroy(gameObject);
            }
            
            blackboardData.SetValuesOnBlackboard(blackboard);
        }
        
        public Blackboard GetBlackboard() => blackboard;
        
        public void Debug() => blackboard.Debug();
        
        public void RegisterExpert(IExpert expert) => arbiter.RegisterExpert(expert);
        public void DeregisterExpert(IExpert expert) => arbiter.DeregisterExpert(expert);

        void Update() {
            // Execute all agreed actions form the current iteration
            foreach (var action in arbiter.BlackboardIteration(blackboard)) {
                action();
            }
        }
        
        private void OnDestroy() {
            if (instance == this) instance = null;
        }
    }
}