//Lucas Davis & Sasha Koroleva

using System.Collections.Generic;
using Audio.SFX;
using UnityEngine;
using Characters.NPC.Behavior_Tree;
using Characters.NPC.Behavior_Tree.Strategies;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using UnityEngine.AI;

namespace Characters.NPC.Controllers
{
    public class DogController : MonoBehaviour {
        [Header("Animation")]
        private Animator animator;

        [Header("Auido")] 
        [SerializeField] AudioClip barkSfx;
        [SerializeField] AudioClip happySfx;
        [SerializeField] AudioClip whineSfx;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
        [Header("Movement")]
        [SerializeField] float speed = 2f;
        [SerializeField] float stoppingDistance = 2.0f;
        [SerializeField] List<Transform> points;
        private NavMeshAgent agent;
        
        BehaviorTree tree;
        
        private Transform target;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        Blackboard blackboard;
        BlackboardKey followKey, moveKey, stayKey;
        
        GameObject exit;
        
        void Awake() {
            exit = GameObject.Find("Door Out");
            exit.GetComponent<CabinExit>().enabled = false;
            
            // Navmesh agent
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            
            if (target == null) {
                target = GameObject.Find("Player").GetComponent<Transform>();
            }
            
            blackboard = controller.GetBlackboard();
            
            followKey = blackboard.GetOrRegisterKey("FollowCommand");
            moveKey = blackboard.GetOrRegisterKey("MoveCommand");
            stayKey = blackboard.GetOrRegisterKey("StayCommand");

            DefineBehavior();
        }

        
        void Start() {
            animator = GetComponentInChildren<Animator>();
        }
        
        void Update() {
            UpdateAnimation();
            tree.Process();
        }

        void DefineBehavior()
        {
            //follow sequence
            tree = new BehaviorTree("dog");
            PrioritySelector actions = new PrioritySelector("logic");
            Sequence followPlayer = new Sequence("followPlayer", 100);
            followPlayer.AddChild(new Leaf("commandFollow?", new Condition( () => blackboard.TryGetValue(followKey, out bool value) && value )));
            followPlayer.AddChild(new Leaf("SFX", new SingleFire(
                new ActionStrategy( () => sfxManager.PlaySFX(happySfx, transform, 1f )), 
                blackboard.GetOrRegisterKey("HasHappy")
                )
            ));
            followPlayer.AddChild(new Leaf("moveToPlayer", new MoveToTarget(transform, agent, target, speed, stoppingDistance)));
            followPlayer.AddChild(new Leaf("Delay", new WaitSeconds(0.5f)));
            actions.AddChild(followPlayer);
            
            //move sequence
            Sequence moveAway = new Sequence("moveAway", 50);
            moveAway.AddChild(new Leaf("commandMoveAway?", new Condition( () => blackboard.TryGetValue(moveKey, out bool value) && value)));
            moveAway.AddChild(new Leaf("SFX", new SingleFire(
                new ActionStrategy(() => sfxManager.PlaySFX(whineSfx, transform, 1f )), 
                blackboard.GetOrRegisterKey("HasWhined")
                )
            ));
            moveAway.AddChild(new Leaf("Heel; move to the player", new SingleFire(
                new MoveToTarget(transform, agent, target, speed, stoppingDistance), 
                blackboard.GetOrRegisterKey("HasMoved")
                )
            ));
            moveAway.AddChild(new Leaf("Delay", new WaitSeconds(0.5f)));
            actions.AddChild(moveAway);
            
            //sit sequence
            Sequence sit = new Sequence("Stay", 10);
            sit.AddChild(new Leaf("commandStay?", new Condition( () => blackboard.TryGetValue(stayKey, out bool value) && value)));
            sit.AddChild(new Leaf("SFX", new SingleFire(
                new ActionStrategy( () => sfxManager.PlaySFX(barkSfx, transform, 1f )),
                blackboard.GetOrRegisterKey("HasBarked")
                )
            ));
            sit.AddChild(new Leaf("Make the dog stay", new ActionStrategy( () => { agent.ResetPath(); agent.velocity = Vector3.zero; })));
            sit.AddChild(new Leaf("Wait", new WaitSeconds(0.5f)));
            actions.AddChild(sit);
            
            //idle sequence
            Sequence idle = new Sequence("idle");
            idle.AddChild(new Leaf("Move a set of points", new PatrolPoints(agent, points, speed)));
            idle.AddChild(new Leaf("idling",new WaitSeconds(1f)));
            actions.AddChild(idle);
            
            tree.AddChild(actions);

        }
        
        void UpdateAnimation() {
            Vector2 velocity = agent.velocity;
            float magnitude = velocity.magnitude;
            
            Vector2 dir = magnitude > 0.1f ? velocity.normalized : Vector2.zero;
            
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
            animator.SetFloat("MoveMagnitude", magnitude);

            if (magnitude < 0.1f && dir == Vector2.zero) {
                animator.SetFloat("LastMoveX", dir.x);
                animator.SetFloat("LastMoveY", dir.y);
            }
        }
    }
}