using UnityEngine;

namespace Characters.NPC.Behavior_Tree.Strategies {
    /// <summary>
    /// Wait
    /// </summary>
    public class WaitSeconds : IStrategy {
        private readonly float waitTime;
        private float timer;

        public WaitSeconds(float waitTime) {
            this.waitTime = waitTime;
        }

        public Nodes.Status Process() {
            timer += Time.deltaTime;
            if (timer >= waitTime) return Nodes.Status.Success;
            
            return Nodes.Status.Running;
        }
        
        public void Reset() => timer = 0f;
    }
}