using System;
using System.Collections.Generic;
using Characters.NPC.Behavior_Tree.Strategies.Conditional;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Characters.NPC.Behavior_Tree.Strategies {
    public class Investigate : IStrategy {
        readonly NavMeshAgent agent;
        Vector2 area;
        readonly Func<Vector2> getPosition;
        readonly float radius;
        readonly float speed;
        readonly float interval;
        private bool isMoving;
        private List<Vector2> positions = new List<Vector2>();
        private int counter;
        private float timer;

        public Investigate(NavMeshAgent agent, Vector2 area, float radius, float speed, float interval) {
            this.agent = agent;
            this.area = area;
            this.radius = radius;
            this.speed = speed;
            this.interval = interval;
        }

        public Investigate(NavMeshAgent agent, Func<Vector2> getPosition, float radius, float speed, float interval) {
            this.agent = agent;
            this.getPosition = getPosition;
            this.radius = radius;
            this.speed = speed;
            this.interval = interval;
        }

        public Nodes.Status Process() {
            timer += Time.deltaTime;
            
            if (counter > positions.Count) return Nodes.Status.Success;
            
            if (isMoving) {
                if (Conditions.ReachedTarget(agent)) {
                    counter++;
                    isMoving = false;
                }
                
                return Nodes.Status.Running;
            }

            if (timer <= interval) return Nodes.Status.Running;
            
            timer = 0;

            if (positions.Count == 0) {
                GeneratePositions();
            }
            
            agent.SetDestination(positions[counter]);
            agent.speed = speed;
            isMoving = true;

            counter++;
            
            return Nodes.Status.Running;
        }

        public void Reset() {
            counter = 0;
            if (positions.Count != 0) positions.Clear();
            isMoving = false;
        }

        private void GeneratePositions() {
            NavMeshHit hit;
            for (int i = 0; i < 3; i++) {
                if (area == default) area = getPosition();
                
                Vector2 target = area + Random.insideUnitCircle * radius;

                if (NavMesh.SamplePosition(target, out hit, radius, NavMesh.AllAreas)) {
                    positions.Add(hit.position);
                }
            }
        }
    }
}