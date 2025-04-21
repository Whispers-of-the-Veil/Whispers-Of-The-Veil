using System.Collections;
using System.Collections.Generic;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using UnityEngine;

namespace Characters.NPC {
    public class SoundExpert : MonoBehaviour, IExpert {
        public static SoundExpert instance;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }

        Blackboard blackboard;
        BlackboardKey soundKey, positionKey;

        Vector2 position = Vector2.zero;
        float time;
        
        enum States { Idle, Reporting, WaitingToReset }
        States state;

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
            state = States.Idle;
            
            blackboard = controller.GetBlackboard();
            controller.RegisterExpert(this);
            
            soundKey = blackboard.GetOrRegisterKey("SoundMade");
            positionKey = blackboard.GetOrRegisterKey("SoundsPosition");
        }

        public void ReportSound(Vector2 position) {
            this.position = position;
            state = States.Reporting;
            time = Time.time + 2f;
        }

        public int GetInsistence(Blackboard blackboard) {
            return state != States.Idle ? 100 : 0;
        }

        public void Execute(Blackboard blackboard) {
            switch (state) {
                case States.Reporting:
                    blackboard.AddAction(() => {
                        blackboard.SetValue(soundKey, true);
                        blackboard.SetValue(positionKey, position);
                    });
                    
                    state = States.WaitingToReset;
                    
                    break;

                case States.WaitingToReset:
                    if (Time.time >= time) {
                        blackboard.AddAction(() => {
                            blackboard.SetValue(soundKey, false);
                        });
                        
                        state = States.Idle;
                    }
                    
                    break;
            }
        }
    }
}