//Lucas Davis & Sasha Koroleva
using UnityEngine;
using Characters.Enemies.Behavior_Tree;
using Characters.Enemies.Behavior_Tree.Strategies;
using Characters.Enemies.Behavior_Tree.Strategies.Conditional;
using UnityEngine.AI;
using Characters.Player.Voice;

namespace Characters.Enemies.Controllers
{
    
    public class DogController : MonoBehaviour
    {
        [Header("Animation")]
        private Animator animator;
        
        [Header("Movement")]
        [SerializeField] float speed = 2f;
        [SerializeField] private float stoppingDistance = 2.0f;
        public NavMeshAgent agent;
        
        BehaviorTree tree;
        
        private Transform target;
        private bool follow;
        private bool move;
        private bool sitting;
        
        void Awake() {
            // Navmesh agent
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            
            if (target == null) {
                target = GameObject.Find("Player").GetComponent<Transform>();
            }

            DefineBehavior();
        }

        
        void Start() {
            animator = GetComponentInChildren<Animator>();
        }
        
        void Update() {
            UpdateAnimation();
            tree.Process();
        }
        
        void OnEnable()
        {
            Voice.OnCommandRecognized += OnVoiceCommand;
        }

        void OnDisable()
        {
            Voice.OnCommandRecognized -= OnVoiceCommand;
        }
        
        private void OnVoiceCommand(string command)
        {
            if (command.ToLower().Trim() == "follow")
            {
                follow = true;
            }
            follow = false;
            
            if (command.ToLower().Trim() == "move")
            {
                move = true;
            }
            move = false;
            
            if (command.ToLower().Trim() == "sit")
            {
                sitting = true;
            }
            sitting = false;
        }


        void DefineBehavior()
        {
            //follow sequence
            tree = new BehaviorTree("dog");
            PrioritySelector actions = new PrioritySelector("logic");
            Sequence followPlayer = new Sequence("followPlayer", 100);
            followPlayer.AddChild(new Leaf("commandFollow?", new Condition(() => follow)));
            followPlayer.AddChild(new Leaf("moveToPlayer", new MoveToTarget(transform, agent, target, speed, stoppingDistance )));
            actions.AddChild(followPlayer);
            
            //move sequence
            Sequence moveAway = new Sequence("moveAway", 50);
            moveAway.AddChild(new Leaf("commandMoveAway?", new Condition(() => move)));
            RandomPicker random = new RandomPicker("random");
            random.AddChild(new Leaf("moveRight", new MoveToTarget(transform, agent, new Vector2(2,0) + (Vector2)transform.position, speed, stoppingDistance)));
            random.AddChild(new Leaf("moveLeft", new MoveToTarget(transform, agent, new Vector2(-2,0) + (Vector2)transform.position, speed, stoppingDistance)));
            moveAway.AddChild(random);
            actions.AddChild(moveAway);
            
            //sit sequence
            Sequence sit = new Sequence("sit", 10);
            sit.AddChild(new Leaf("commandSit?", new Condition(() => sitting)));
            sit.AddChild(new Leaf("wait", new WaitSeconds(120)));
            sit.AddChild(new Leaf("moveToPlayer", new MoveToTarget(transform, agent, target, speed, stoppingDistance)));
            actions.AddChild(sit);
            
            //idle sequence
            Sequence idle = new Sequence("idle");
            idle.AddChild(new Leaf("idling",new WaitSeconds(0.5f)));
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