using System.Collections;
using Characters.Enemies.Behavior_Tree;
using Characters.Enemies.Behavior_Tree.Strategies;
using Characters.Enemies.Behavior_Tree.Strategies.Conditional;
using Characters.Player;
using UnityEngine;
using UnityEngine.AI;
using Audio.SFX;

namespace Characters.Enemies.Controllers {
    public class WolfController : MonoBehaviour {
        [Header("Audio")]
        [SerializeField] AudioClip idleSfx;
        [SerializeField] AudioClip walkSfx;
        [SerializeField] AudioClip alertSfx;
        [SerializeField] AudioClip angrySfx;
        [SerializeField] AudioClip deathSfx;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
        [Header("Combat")]
        [SerializeField] Transform target;
        [SerializeField] private float stoppingDistance = 2.0f;
        [SerializeField] float attackRange;
        [SerializeField] float attackSpeed;
        [SerializeField] float attackInterval;
        public float hurtDistance = 0.25f;
        private float timeOfLastAttack = 0;
        private bool hasStopped = false;
        private EnemyStats stats = null;
        
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
        
        BehaviorTree tree;
        
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
            
            GetReferences();

            DefineBehavior();
        }

        void Start() {

        }

        void Update() {
            tree.Process();
        }

        void DefineBehavior() {
            tree = new BehaviorTree("Wolf");

            PrioritySelector actions = new PrioritySelector("Logic");
            
            // Attack Logic --------------------------------------
            
            // Target isnt updating its position
            RandomSelector randomAttack = new RandomSelector("Ramdom");
            Sequence attack = new Sequence("Normal Attack Pattern");
            attack.AddChild(new Leaf("Move in for attack", new MoveToTarget(transform, agent, target, attackSpeed, stoppingDistance)));
            attack.AddChild(new Leaf("Attack!", new ActionStrategy(AttackPlayer)));
            randomAttack.AddChild(attack);

            Sequence dashAttack = new Sequence("Dash Attack Pattern");
            dashAttack.AddChild(new Leaf("Move in for attack", new MoveToTarget(transform, agent, target, attackSpeed * 2, stoppingDistance)));
            dashAttack.AddChild(new Leaf("Attack!", new ActionStrategy(AttackPlayer)));
            randomAttack.AddChild(dashAttack);
                
            Sequence attackPlayer = new Sequence("Attack", 150);
            attackPlayer.AddChild(new Leaf("is Player In Range?", new Condition(() => Conditions.InRange(transform, attackRange))));
            attackPlayer.AddChild(new Leaf("Strafe while waiting to attack", new StrafeDelay(agent, target, 1f, speed, attackInterval)));
            attackPlayer.AddChild(new Leaf("Emote", new ActionStrategy(() => StartCoroutine(ShowEmote(angryEmote)))));
            attackPlayer.AddChild(randomAttack);
            actions.AddChild(attackPlayer);

            // Senses --------------------------------------
            // Sequence seenPlayer = new Sequence("Seen Player", 100);
            // seenPlayer.AddChild(new Leaf("is Player Invisible?", new Condition(() => !target.GetComponent<PlayerStats>().isInvisible)));
            // seenPlayer.AddChild(new Leaf("is Player In Range?", new Condition(() => Conditions.InRange(transform, sightRange))));
            // seenPlayer.AddChild(new Leaf("Move to Player", new MoveToTarget(agent, target, speed)));
            // actions.AddChild(seenPlayer);
            
            // Sequence heardNoise = new Sequence("Investigate Noise", 50);
            // heardNoise.AddChild(new Leaf("Heard Sound?", new Condition(Conditions.HeardSound)));
            // heardNoise.AddChild(new Leaf("is Sound in Range?", new Condition(() => Conditions.InRange(transform, hearingRange))));
            // heardNoise.AddChild(new Leaf("Emote", new ActionStrategy(() => StartCoroutine(ShowEmote(alertEmote)))));
            // heardNoise.AddChild(new Leaf("Move to sound", new MoveToTarget(agent, )));
            // heardNoise.AddChild(new Leaf("Investigate", new Investigate()));
            // actions.AddChild(heardNoise);
            
            // Default behavior
            Sequence patrol = new Sequence("Patrol");
            patrol.AddChild(new Leaf("Patrol", new Patrol(agent, patrolArea, patrolRadius, speed)));
            patrol.AddChild(new Leaf("Delay", new WaitSeconds(5f)));
            actions.AddChild(patrol);
            
            tree.AddChild(actions);
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