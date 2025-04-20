using UnityEngine;
namespace Characters.NPC.BlackboardSystem.Control {
    public class BlackboardController : MonoBehaviour {
        public static BlackboardController instance;
        
        [SerializeField] BlackboardData blackboardData;
        readonly Blackboard blackboard = new Blackboard();
        readonly Arbiter arbiter = new Arbiter();

        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
            }
            
            blackboardData.SetValuesOnBlackboard(blackboard);
            blackboard.Debug();
        }
        
        public Blackboard GetBlackboard() => blackboard;
        
        public void RegisterExpert(IExpert expert) => arbiter.RegisterExpert(expert);
        // TODO Add DeregisterExpert method

        void Update() {
            // Execute all agreed actions form the current iteration
            foreach (var action in arbiter.BlackboardIteration(blackboard)) {
                action();
            }
        }
    }
}