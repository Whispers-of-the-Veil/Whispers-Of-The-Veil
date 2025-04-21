using Characters.NPC.BlackboardSystem;

namespace Characters.NPC.Commands {
    public class MoveCommand : ICommand {
        private readonly BlackboardKey key, flag;

        public MoveCommand(BlackboardKey key, BlackboardKey flagKey) {
            this.key = key;
            this.flag = flagKey;
        }

        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                blackboard.SetValue(flag, false);   // Reset the flag for another system (SingleFire)
                blackboard.SetValue(key, true);     // Set the flag for this command
            });
        }
    }
}