using System.Collections.Generic;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using Characters.NPC.Commands;
using Characters.Player.Speech;
using UnityEngine;

namespace Characters.MainBoss {
    public class StunnedExpert : MonoBehaviour, IExpert {
        public static StunnedExpert instance;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        private ICommand pendingCommand;
        Blackboard blackboard;
        private Dictionary<string, ICommand> commands;
        BlackboardKey stunnedKey;

        private bool reset;

        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            blackboard = controller.GetBlackboard();
            controller.RegisterExpert(this);

            stunnedKey = blackboard.GetOrRegisterKey("Stunned");

            commands = new Dictionary<string, ICommand>() {
                { "silence", new StunnedCommand(stunnedKey) },
                { "reset", new ResetCommand(new List<BlackboardKey>() { stunnedKey }) },
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

        public void Reset() => reset = true;
        
        public int GetInsistence(Blackboard blackboard) => pendingCommand != null || reset ? 80 : 0;

        public void Execute(Blackboard blackboard) {
            if (reset) {
                commands["reset"].Execute(blackboard);
                reset = false;
                pendingCommand = null;
            }
            else {
                pendingCommand?.Execute(blackboard);
                pendingCommand = null;
            }
        }
    }
}