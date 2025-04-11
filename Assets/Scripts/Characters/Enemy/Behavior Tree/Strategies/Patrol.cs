using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Characters.Enemy.Behavior_Tree.Strategies {
    public class Patrol : IStrategy {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly Transform area;
        readonly float radius;
        readonly float speed;
        private bool isPatroling;

        public Patrol(Transform entity, NavMeshAgent agent, Transform area, float radius, float speed) {
            this.entity = entity;
            this.agent = agent;
            this.area = area;
            this.radius = radius;
            this.speed = speed;
        }

        public Nodes.Status Process() {
            if (isPatroling) {
                return Nodes.Status.Running;
            }

            Vector2 target = (Vector2)area.position + Random.insideUnitCircle * radius;
            NavMeshHit hit;

            entity.LookAt(target);

            if (NavMesh.SamplePosition(target, out hit, radius, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;

                isPatroling = true;

                return Nodes.Status.Running;
            }

            return Nodes.Status.Failure;
        }

        public void Reset() => isPatroling = false;
    }
}