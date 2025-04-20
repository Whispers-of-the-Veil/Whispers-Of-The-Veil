using Characters.NPC.BlackboardSystem;

namespace Characters.NPC.Commands {
    public class SitCommand : ICommand {
        readonly BlackboardKey key;

        public SitCommand(BlackboardKey key) {
            this.key = key;
        }

        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                blackboard.SetValue(key, true);
            });
        }
    }
}