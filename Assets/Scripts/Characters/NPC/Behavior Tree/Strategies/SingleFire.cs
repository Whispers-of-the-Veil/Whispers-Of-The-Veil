using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;

namespace Characters.NPC.Behavior_Tree.Strategies {
    /// <summary>
    /// This strategy encapsulates another strategy making sure it only is processed once.
    /// It depends on another system to reset it using the key on the blackboard
    /// </summary>
    public class SingleFire : IStrategy, IExpert {
        readonly IStrategy inner;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        Blackboard blackboard;
        BlackboardKey key;

        private bool hasFired;

        public SingleFire(IStrategy inner, BlackboardKey key) {
            this.inner = inner;
            
            blackboard = controller.GetBlackboard();
            controller.RegisterExpert(this);
            this.key = key;
        }

        public int GetInsistence(Blackboard blackboard) => hasFired ? 0 : 10;
        
        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                blackboard.SetValue(key, true);
            });
        }
        
        public Nodes.Status Process() {
            if (hasFired) {
                if (!(blackboard.TryGetValue(key, out bool value) && value)) {
                    hasFired = false;
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