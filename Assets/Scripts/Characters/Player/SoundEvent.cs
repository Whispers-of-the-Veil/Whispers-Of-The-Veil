using System;
using System.Collections;
using UnityEngine;

namespace Characters.Player {
    public static class SoundEvent  {
        public static bool noiseMade;
        public static Transform noisePosition;

        public static void ReportNoise(Transform position) {
            noiseMade = true;
            noisePosition = position;
        }

        public static void ClearReport() => noiseMade = false;
    }
}