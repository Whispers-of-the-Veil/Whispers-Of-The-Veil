using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using UnityEngine;

namespace Characters.NPC.Behavior_Tree.Strategies {
    /// <summary>
    /// This strategy encapsulates another strategy making sure it only is processed once.
    /// It depends on another system to reset it using the key on the blackboard
    /// </summary>
    public class SingleFire : IStrategy {
        readonly IStrategy inner;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        Blackboard blackboard;
        BlackboardKey key;
        
        public SingleFireExpert singleFireExpert {
            get => SingleFireExpert.instance;
        }

        private bool hasFired;
        private bool enqueued;

        public SingleFire(IStrategy inner, BlackboardKey key) {
            blackboard = controller.GetBlackboard();
            
            this.inner = inner;
            this.key = key;
        }
        
        public Nodes.Status Process() {
            if (hasFired) {
                if (!enqueued) {
                    enqueued = true;
                    
                    singleFireExpert.EnqueueAction(() => {
                        blackboard.SetValue(key, true);
                    });
                }
                
                if (!(blackboard.TryGetValue(key, out bool value) && value)) {
                    hasFired = false;
                    enqueued = false;
                }
                
                return Nodes.Status.Success;
            }
            
            var result = inner.Process();
            
            if (result == Nodes.Status.Success || result == Nodes.Status.Failure) {
                hasFired = true;
            }
            return result;
        }
        
        public void Reset() => inner.Reset();
    }
}