

using System.Collections.Generic;
using Characters.NPC.Behavior_Tree.Strategies.Conditional;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.NPC.Behavior_Tree.Strategies {
    public class PatrolPoints : IStrategy {
        readonly NavMeshAgent agent;
        readonly List<Transform> points;
        readonly float speed;
        private int counter;
        private bool isMoving;
        
        public PatrolPoints(NavMeshAgent agent, List<Transform> points, float speed) {
            this.agent = agent;
            this.points = points;
            this.speed = speed;
            
            counter = 0;
        }

        public Nodes.Status Process() {
            if (isMoving) {
                if (Conditions.ReachedTarget(agent)) isMoving = false;
                if (counter >= points.Count) return Nodes.Status.Success;
                
                return Nodes.Status.Running;
            }

            NavMeshHit hit;

            if (NavMesh.SamplePosition((Vector2)points[counter].position, out hit, 0.5f, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;
                
                isMoving = true;
                counter++;

                return Nodes.Status.Running;
            }

            return Nodes.Status.Failure;
        }

        public void Reset() {
            counter = 0;
            isMoving = false;
        }
    }
}