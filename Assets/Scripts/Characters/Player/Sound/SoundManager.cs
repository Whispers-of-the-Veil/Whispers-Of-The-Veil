using UnityEngine;

namespace Characters.Player.Sound {
    public static class SoundManager {
        public static Report Report { get; private set; }
        
        // public static Report Report {
        //     get {
        //         return Report ??= new Report {
        //             Position = Vector2.zero,
        //             Timestamp = Time.time,
        //             Duration = 0f
        //         };
        //     }
        //     
        //     private set { }
        // }

        public static void ReportSound(Vector2 position) {
            Report = new Report {
                Position = position,    // Record the position of the event
                Timestamp = Time.time,  // Record the current time
                Duration = 2f           // Set a duration for the report to be active for
            };
        }
    }
}