using System.Collections.Generic;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using Characters.NPC.Commands;
using UnityEngine;
using Characters.Player.Speech;

namespace Characters.NPC {
    public class VoiceExpert : MonoBehaviour, IExpert {
        public static VoiceExpert instance;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        private ICommand pendingCommand;
        Blackboard blackboard;
        private Dictionary<string, ICommand> commands;
        
        BlackboardKey followKey, moveKey, stayKey, movedKey, barkedKey, whineKey, happyKey, hasRecordedKey;

        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else {
                Destroy(gameObject);
            }
        }

        void Start() {
            blackboard = controller.GetBlackboard();
            controller.RegisterExpert(this);

            followKey = blackboard.GetOrRegisterKey("FollowCommand");
            moveKey = blackboard.GetOrRegisterKey("MoveCommand");
            stayKey = blackboard.GetOrRegisterKey("StayCommand");
            
            movedKey = blackboard.GetOrRegisterKey("HasMoved");
            
            barkedKey = blackboard.GetOrRegisterKey("HasBarked");
            whineKey = blackboard.GetOrRegisterKey("HasWhined");
            happyKey = blackboard.GetOrRegisterKey("HasHappy");

            hasRecordedKey = blackboard.GetOrRegisterKey("HasRecorded");

            commands = new Dictionary<string, ICommand>() {
                { "follow", new FollowCommand(followKey) },
                { "move", new MoveCommand(moveKey) },
                { "stay", new StayCommand(stayKey) },
                { "reset", new ResetCommand(new List<BlackboardKey>() {
                    followKey, moveKey, stayKey, movedKey, barkedKey, whineKey, happyKey, hasRecordedKey
                    })
                },
            };
        }
        
        void OnEnable() => Voice.OnCommandRecognized += OnVoiceCommand;
        void OnDisable() => Voice.OnCommandRecognized -= OnVoiceCommand;

        private void OnVoiceCommand(string command) {
            if (commands.TryGetValue(command, out var cmd)) {
                pendingCommand = cmd;
            }
            else {
                Debug.LogWarning("No command recognized");
            }
        }

        public int GetInsistence(Blackboard blackboard) => pendingCommand != null ? 80 : 0;

        public void Execute(Blackboard blackboard) {
            commands["reset"].Execute(blackboard);  // Reset all of the flags
            pendingCommand?.Execute(blackboard);    // Set the approriate flag
            pendingCommand = null;                  // Clear pending commmand
        }
    }
}