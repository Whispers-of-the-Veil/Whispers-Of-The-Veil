// Lucas

using Characters.NPC.BlackboardSystem;

namespace Characters.NPC.Commands {
    public class FollowCommand : ICommand {
        readonly BlackboardKey key;

        public FollowCommand(BlackboardKey key) {
            this.key = key;
        }
        
        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                blackboard.SetValue(key, true);
            });
        }
    }
}