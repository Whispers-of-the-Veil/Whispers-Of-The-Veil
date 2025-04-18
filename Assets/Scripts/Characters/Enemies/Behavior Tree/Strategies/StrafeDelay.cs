using System;
using UnityEngine;
using UnityEngine.AI;
using Characters.Enemies.Behavior_Tree.Strategies.Conditional;
using UnityEditor;
using Random = UnityEngine.Random;

namespace Characters.Enemies.Behavior_Tree.Strategies {
    /// <summary>
    /// Strafe around the target until we are ready to attack
    /// </summary>
    public class StrafeDelay : IStrategy {
        readonly NavMeshAgent agent;
        readonly Transform target;
        readonly float distance;
        readonly float speed;
        readonly float interval;
        private bool isMoving;
        private float timer;
        
        public StrafeDelay(NavMeshAgent agent, Transform target, float distance, float speed, float interval) {
            this.agent = agent;
            this.target = target;
            this.distance = distance;
            this.speed = speed;
            this.interval = interval;
        }

        public Nodes.Status Process() {
            timer += Time.deltaTime;;
            if (timer >= interval) return Nodes.Status.Success;
            
            if (isMoving) {
                if (Conditions.ReachedTarget(agent)) isMoving = false;
                
                return Nodes.Status.Running;
            }
            
            Vector2 point = GetPoint();
            NavMeshHit hit;

            if (NavMesh.SamplePosition(point, out hit, distance, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;
                
                isMoving = true;
                return Nodes.Status.Running;
            }
            
            return Nodes.Status.Failure;
        }

        public void Reset() {
            timer = 0f;
            isMoving = false;
        }

        // Get a random position around the target while maintaning a distance
        private Vector2 GetPoint() {
            Vector2 center = target.position;
            float angle = Random.Range(0f, 2 * Mathf.PI);

            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }
    }
}