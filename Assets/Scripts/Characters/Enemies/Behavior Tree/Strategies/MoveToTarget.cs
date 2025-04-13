using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Enemies.Behavior_Tree.Strategies {
    public class MoveToTarget : IStrategy {
        private readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly Transform target;
        readonly float speed;
        private bool isMoving;

        public MoveToTarget(NavMeshAgent agent, Transform target, float speed) {
            this.agent = agent;
            this.target = target;
            this.speed = speed;
        }

        public Nodes.Status Process() {
            if (isMoving) {
                if (ReachedTarget()) return Nodes.Status.Success;
                
                return Nodes.Status.Running;
            }
            
            NavMeshHit hit;
            
            if (NavMesh.SamplePosition(target.position, out hit, 1f, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;
                
                isMoving = true;
                return Nodes.Status.Running;
            }
            
            return Nodes.Status.Failure;
        }
        
        public void Reset() => isMoving = false;
        
        private bool ReachedTarget() {
            if (!agent.pathPending) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}