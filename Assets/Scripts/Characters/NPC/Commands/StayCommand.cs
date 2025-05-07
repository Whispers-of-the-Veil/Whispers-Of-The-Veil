// Lucas

using Characters.NPC.BlackboardSystem;

namespace Characters.NPC.Commands {
    public class StayCommand : ICommand {
        readonly BlackboardKey key;

        public StayCommand(BlackboardKey key) {
            this.key = key;
        }

        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                blackboard.SetValue(key, true);
            });
        }
    }
}