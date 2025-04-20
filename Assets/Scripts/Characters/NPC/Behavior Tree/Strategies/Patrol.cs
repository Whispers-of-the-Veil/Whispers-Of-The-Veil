using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Characters.NPC.Behavior_Tree.Strategies.Conditional;

namespace Characters.NPC.Behavior_Tree.Strategies {
    public class Patrol : IStrategy {
        readonly NavMeshAgent agent;
        readonly Transform area;
        readonly float radius;
        readonly float speed;
        
        private Vector2 target;
        private bool hasTarget;

        public Patrol(NavMeshAgent agent, Transform area, float radius, float speed) {
            this.agent = agent;
            this.area = area;
            this.radius = radius;
            this.speed = speed;
        }

        public Nodes.Status Process() {
            if (hasTarget) {
                if (Conditions.ReachedTarget(agent)) return Nodes.Status.Success;
                
                return Nodes.Status.Running;
            }
            
            NavMeshHit hit;
            target = (Vector2)area.position + Random.insideUnitCircle * radius;

            if (NavMesh.SamplePosition(target, out hit, radius, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;
                
                hasTarget = true;
                return Nodes.Status.Running;
            }
            
            return Nodes.Status.Failure;
        }

        public void Reset() => hasTarget = false;
    }
}