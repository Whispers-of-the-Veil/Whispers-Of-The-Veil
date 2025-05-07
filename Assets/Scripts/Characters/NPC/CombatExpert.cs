// Lucas

using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using Management;
using UnityEngine;

namespace Characters.NPC {
    public class CombatExpert : MonoBehaviour, IExpert {
        public static CombatExpert instance;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }

        Blackboard blackboard;
        BlackboardKey combatKey;
        
        private enum States { Idle, PendingReport }
        private States state;

        private bool inCombat;
        
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
        }

        void Start() {
            state = States.Idle;
            
            inCombat = false;
            
            blackboard = controller.GetBlackboard();
            controller.RegisterExpert(this);
            
            combatKey = blackboard.GetOrRegisterKey("InCombat");
        }

        public void ReportCombat(bool inCombat) {
            state = States.PendingReport;
            this.inCombat = inCombat;
        }
        
        public int GetInsistence(Blackboard blackboard) => state == States.PendingReport ? 5 : 0;

        public void Execute(Blackboard blackboard) {
            state = States.Idle;
            
            blackboard.AddAction(() => {
                blackboard.SetValue(combatKey, inCombat);
            });
        }
        
        private void OnDestroy() {
            if (instance == this) instance = null;
        }
    }
}