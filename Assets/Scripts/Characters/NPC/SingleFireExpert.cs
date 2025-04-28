using System;
using System.Collections.Generic;
using System.Linq;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using UnityEngine;

namespace Characters.NPC {
    public class SingleFireExpert : MonoBehaviour, IExpert {
        public static SingleFireExpert instance;
        private Queue<Action> actions = new Queue<Action>();
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        Blackboard blackboard;

        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
            }
        }

        void Start() {
            blackboard = controller.GetBlackboard();
            controller.RegisterExpert(this);
        }

        public void EnqueueAction(Action action) => actions.Enqueue(action);

        public int GetInsistence(Blackboard blackboard) => actions.Any() ? 10 : 0;

        public void Execute(Blackboard blackboard) {
            Action action = actions.Dequeue();

            blackboard.AddAction(action);
        }
    }
}