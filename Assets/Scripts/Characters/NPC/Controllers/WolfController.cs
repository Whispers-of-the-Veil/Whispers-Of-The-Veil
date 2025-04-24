using System.Collections;
using Characters.Player;
using UnityEngine;
using UnityEngine.AI;
using Audio.SFX;
using Characters.NPC;
using Characters.NPC.Behavior_Tree;
using Characters.NPC.Behavior_Tree.Strategies;
using Characters.NPC.Behavior_Tree.Strategies.Conditional;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;

namespace Characters.Enemies.Controllers {
    public class WolfController : MonoBehaviour {
        [Header("Animation")]
        private Animator animator;
        
        [Header("Audio")]
        [SerializeField] AudioClip howlingSfx;
        [SerializeField] AudioClip walkSfx;
        [SerializeField] AudioClip alertSfx;
        [SerializeField] AudioClip angrySfx;
        [SerializeField] AudioClip attackSfx;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
        [Header("Combat")]
        [SerializeField] private float stoppingDistance = 2.0f;
        [SerializeField] float attackRange;
        [SerializeField] float attackSpeed;
        [SerializeField] float attackInterval;
        public float hurtDistance = 0.25f;
        private float timeOfLastAttack = 0;
        private bool hasStopped = false;
        private EnemyStats stats = null;
        private Transform target;
        
        [Header("Emotes")]
        private GameObject alertEmote;
        private GameObject angryEmote;
        private GameObject frustratedEmote;
        
        [Header("Movement")]
        [SerializeField] float speed = 2f;
        [SerializeField] float patrolRadius = 5f;
        [SerializeField] float minIvenstigateDistance; 
        [SerializeField] float maxIvenstigateDistance;
        [SerializeField] private Transform patrolArea;
        public NavMeshAgent agent;
        
        [Header("Senses")]
        [SerializeField] public float sightRange;       // How far can this enemy see
        [SerializeField] public float hearingRange;     // How far can this enemy hear
        [SerializeField] public float searchRadius;
        
        BehaviorTree tree;
        public BlackboardController controller {
            get => BlackboardController.instance;
        }
        Blackboard blackboard;
        BlackboardKey soundKey, positionKey;
        
        void Awake() {
            // Navmesh agent
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            
            if (target == null) {
                target = GameObject.Find("Player").GetComponent<Transform>();
            }
            
            alertEmote = GameObject.Find("Emotes/Alert");
            angryEmote = GameObject.Find("Emotes/Angry");
            frustratedEmote = GameObject.Find("Emotes/Frustrated");
            
            alertEmote.SetActive(false);
            angryEmote.SetActive(false);
            frustratedEmote.SetActive(false);
            
            blackboard = controller.GetBlackboard();
            soundKey = blackboard.GetOrRegisterKey("SoundMade");
            positionKey = blackboard.GetOrRegisterKey("SoundsPosition");
            
            GetReferences();

            DefineBehavior();
        }

        void Start() {
            animator = GetComponentInChildren<Animator>();
        }

        void Update() {
            UpdateAnimation();
            tree.Process();
        }
        
        void DefineBehavior() {
            tree = new BehaviorTree("Wolf");

            PrioritySelector actions = new PrioritySelector("Logic");
            
            actions.AddChild(CombatLogic());

            // Senses --------------------------------------
            Sequence seenPlayer = new Sequence("Seen Player", 100);
            seenPlayer.AddChild(new Leaf("is Player Invisible?", new Condition(() => !target.GetComponent<PlayerStats>().isInvisible)));
            seenPlayer.AddChild(new Leaf("is Player In Range?", new Condition(() => Conditions.InRange(transform, sightRange))));
            seenPlayer.AddChild(new Leaf("SFX", new ActionStrategy(() => sfxManager.PlaySFX(angrySfx, transform, 1f))));
            seenPlayer.AddChild(new Leaf("Emote", new ActionStrategy(() => StartCoroutine(ShowEmote(frustratedEmote)))));
            seenPlayer.AddChild(new Leaf("Move to Player", new MoveToTarget(transform, agent, target, speed, 2f)));
            actions.AddChild(seenPlayer);
            
            Sequence heardNoise = new Sequence("Investigate Noise", 50);
            heardNoise.AddChild(new Leaf("is there an Active sound report?", new Condition( () => blackboard.TryGetValue(soundKey, out bool value) && value )));
            heardNoise.AddChild(new Leaf("is Sound in Range?", new Condition(() => Conditions.InRange(transform, hearingRange))));
            heardNoise.AddChild(new Leaf("SFX", new ActionStrategy(() => sfxManager.PlaySFX(alertSfx, transform, 1f))));
            heardNoise.AddChild(new Leaf("Emote", new ActionStrategy(() => StartCoroutine(ShowEmote(alertEmote)))));
            heardNoise.AddChild(new Leaf("Delay before investigate", new WaitSeconds(1f)));
            heardNoise.AddChild(new Leaf("Move to sound", new MoveToTarget(transform, agent, () => blackboard.TryGetValue(positionKey, out Vector2 value) ? value : Vector2.zero, speed, 0.25f)));
            heardNoise.AddChild(new Leaf("Investigate Area", new Investigate(agent, () => blackboard.TryGetValue(positionKey, out Vector2 value) ? value : Vector2.zero, searchRadius, speed, 2)));
            actions.AddChild(heardNoise);
            
            // Default behavior
            Sequence patrol = new Sequence("Patrol");
            patrol.AddChild(new Leaf("Patrol", new Patrol(agent, patrolArea, patrolRadius, speed)));
            RandomPicker howl = new RandomPicker("Should I howl?");
            howl.AddChild(new Leaf("Howl", new ActionStrategy(() => sfxManager.PlaySFX(howlingSfx, transform, 1f))));
            howl.AddChild(new Leaf("Don't Howl", new WaitSeconds(0f)));
            patrol.AddChild(howl);
            patrol.AddChild(new Leaf("Delay", new WaitSeconds(5f)));
            actions.AddChild(patrol);
            
            tree.AddChild(actions);
        }

        Sequence CombatLogic() {
            // This is a sequence of attacks that the enemy can preform
            RandomSequence sequenceAttack = new RandomSequence("Random Sequence of attacks");
            // This will randomly pick a type of attack; attack, dash, sequence (in this instance)
            RandomPicker randomAttack = new RandomPicker("Ramdom");
            
            Sequence attack = new Sequence("Normal Attack Pattern");
            attack.AddChild(new Leaf("Move in for attack", new MoveToTarget(transform, agent, target, attackSpeed, stoppingDistance)));
            attack.AddChild(new Leaf("SFX", new ActionStrategy(() => sfxManager.PlaySFX(attackSfx, transform, 1f))));
            attack.AddChild(new Leaf("Did the player move?", new Condition(() => Conditions.InRange(transform, hurtDistance))));
            attack.AddChild(new Leaf("Attack!", new ActionStrategy(AttackPlayer)));
            randomAttack.AddChild(attack);

            Sequence dashAttack = new Sequence("Dash Attack Pattern");
            dashAttack.AddChild(new Leaf("Move in for attack", new MoveToTarget(transform, agent, target, attackSpeed * 2, stoppingDistance)));
            dashAttack.AddChild(new Leaf("SFX", new ActionStrategy(() => sfxManager.PlaySFX(attackSfx, transform, 1f))));
            dashAttack.AddChild(new Leaf("Did the player move?", new Condition(() => Conditions.InRange(transform, hurtDistance))));
            dashAttack.AddChild(new Leaf("Attack!", new ActionStrategy(AttackPlayer)));
            randomAttack.AddChild(dashAttack);
            
            sequenceAttack.AddChild(attack);
            sequenceAttack.AddChild(dashAttack);
            randomAttack.AddChild(sequenceAttack);
                
            Sequence attackPlayer = new Sequence("Attack", 150);
            attackPlayer.AddChild(new Leaf("is Player In Range?", new Condition(() => Conditions.InRange(transform, attackRange))));
            attackPlayer.AddChild(new Leaf("Strafe while waiting to attack", new StrafeDelay(agent, target, 2f, speed, attackInterval)));
            attackPlayer.AddChild(new Leaf("Emote", new ActionStrategy(() => StartCoroutine(ShowEmote(angryEmote)))));
            attackPlayer.AddChild(randomAttack);

            return attackPlayer;
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
        
        /// <summary>
        /// Briefly display a given emote
        /// </summary>
        IEnumerator ShowEmote(GameObject emote) {
            emote.SetActive(true);
            yield return new WaitForSeconds(1);
            emote.SetActive(false);
        }
        
        public void AttackPlayer()
        {
            PlayerStats playerStats = target.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                stats.DealDamage(playerStats);
            }
        }
        
        private void GetReferences()
        {
            //get enemy stats
            stats = GetComponent<EnemyStats>();
        }
    }
}

