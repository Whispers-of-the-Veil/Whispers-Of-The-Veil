// Lucas

using Characters.NPC.BlackboardSystem;
using UnityEngine;

namespace Characters.NPC.Commands {
    public class MoveCommand : ICommand {
        private readonly BlackboardKey key;

        public MoveCommand(BlackboardKey key) {
            this.key = key;
        }

        public void Execute(Blackboard blackboard) {
            blackboard.AddAction(() => {
                blackboard.SetValue(key, true);     // Set the flag for this command
                GameObject.Find("Door Out").GetComponent<CabinExit>().enabled = true;
            });
        }
    }
}