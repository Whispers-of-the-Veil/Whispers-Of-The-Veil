// Lucas

using Characters.NPC.BlackboardSystem;

namespace Characters.NPC.Commands {
    public class StunnedCommand : ICommand {
        readonly BlackboardKey key;

        public StunnedCommand(BlackboardKey key) {
            this.key = key;
        }
        
        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                blackboard.SetValue(key, true);
            });
        }
    }
}