using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using Characters.Enemies.Behavior_Tree.Strategies.Conditional;

namespace Characters.Enemies.Behavior_Tree.Strategies {
    public class MoveToTarget : IStrategy {
        private readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly Transform target;
        private Vector2 targetPosition;
        readonly float speed;
        readonly float stopDistance;
        private bool isMoving;

        public MoveToTarget(Transform entity, NavMeshAgent agent, Transform target, float speed, float stopDistance) {
            this.entity = entity;
            this.agent = agent;
            this.target = target;
            this.speed = speed;
            this.stopDistance = stopDistance;
        }

        public MoveToTarget(Transform entity, NavMeshAgent agent, Vector2 target, float speed, float stopDistance) {
            this.entity = entity;
            this.agent = agent;
            this.targetPosition = target;
            this.speed = speed;
            this.stopDistance = stopDistance;
        }
        
        public Nodes.Status Process() {
            if (isMoving) {
                if (Conditions.ReachedTarget(agent)) return Nodes.Status.Success;
                
                return Nodes.Status.Running;
            }
            
            NavMeshHit hit;

            if (target != null) {
                targetPosition = target.position;
            }
            
            Vector2 directionToTarget = (targetPosition - (Vector2)entity.position).normalized;
            Vector2 stopPosition = targetPosition + (directionToTarget * stopDistance);
            
            if (NavMesh.SamplePosition(stopPosition, out hit, stopDistance, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;
                
                isMoving = true;
                return Nodes.Status.Running;
            }
            
            return Nodes.Status.Failure;
        }
        
        public void Reset() => isMoving = false;
    }
}