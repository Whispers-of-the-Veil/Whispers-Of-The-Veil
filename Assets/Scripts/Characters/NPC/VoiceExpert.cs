using System.Collections.Generic;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using Characters.NPC.Commands;
using UnityEngine;
using Characters.Player.Voice;

namespace Characters.Enemies {
    public class VoiceExpert : MonoBehaviour, IExpert {
        BlackboardKey followKey, moveKey, sitKey;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        private ICommand pendingCommand;
        Blackboard blackboard;
        private Dictionary<string, ICommand> commands;

        void Start() {
            blackboard = controller.GetBlackboard();
            controller.RegisterExpert(this);

            followKey = blackboard.GetOrRegisterKey("FollowCommand");
            moveKey = blackboard.GetOrRegisterKey("SitCommand");
            sitKey = blackboard.GetOrRegisterKey("MoveCommand");

            commands = new Dictionary<string, ICommand>() {
                { "follow", new FollowCommand(followKey) },
                { "move", new MoveCommand(moveKey) },
                { "sit", new SitCommand(sitKey) },
                { "reset", new ResetCommand(new List<BlackboardKey>() { followKey, moveKey, sitKey }) },
            };
        }
        
        void OnEnable() {
            Voice.OnCommandRecognized += OnVoiceCommand;
        }

        void OnDisable() {
            Voice.OnCommandRecognized -= OnVoiceCommand;
        }

        private void OnVoiceCommand(string command) {
            if (commands.TryGetValue(command, out var cmd)) {
                pendingCommand = cmd;
            }
            else {
                Debug.LogWarning("No command recognized");
            }
        }

        public int GetInsistence(Blackboard blackboard) {
            return pendingCommand != null ? 100 : 0;
        }

        public void Execute(Blackboard blackboard) {
            commands["reset"].Execute(blackboard);  // Reset all of the flags
            pendingCommand?.Execute(blackboard);    // Set the approriate flag
            pendingCommand = null;                  // Clear pending commmand
        }
    }
}