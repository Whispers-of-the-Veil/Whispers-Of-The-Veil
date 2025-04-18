using UnityEngine;

namespace Characters.Player.Sound {
    public class Report {
        public Vector2 Position;
        public float Timestamp;
        public float Duration;

        public bool IsActive => Time.time - Timestamp < Duration;
        public Vector2 GetPosition() => Position;
    }
}