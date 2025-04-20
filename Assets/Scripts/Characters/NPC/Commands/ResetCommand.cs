using System.Collections.Generic;
using Characters.NPC.BlackboardSystem;

namespace Characters.NPC.Commands {
    public class ResetCommand : ICommand {
        readonly List<BlackboardKey> keys;

        public ResetCommand(List<BlackboardKey> keys) {
            this.keys = keys;
        }
        
        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                foreach (var key in keys) {
                    blackboard.SetValue(key, false);
                }
            });
        }
    }
}