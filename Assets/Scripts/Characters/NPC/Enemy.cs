// Lucas Davis(10-166)
//Owen Ingram(167-195)

using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using Audio.SFX;
using Characters.Player;


// This is a general class that is attached to every enemy. It doesnt provided any behavior logic, but
// it has conditions and actions that every enemy will have access to.
namespace Characters.NPC {
    public class Enemy : MonoBehaviour {
        [Header("Audio")]
        [SerializeField] AudioClip hurtSfx;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
        private EnemyStats stats = null;
        
        private void Start() {
            GetReferences();
        }
        
        private void GetReferences()
        {
            //get enemy stats
            stats = GetComponent<EnemyStats>();
        }
        
        public void TakeDamage(float damageAmount)
        {
            sfxManager.PlaySFX(hurtSfx, transform, 1f);
            stats.TakeDamage(damageAmount); // Call TakeDamage from EnemyStats
        }
    }
}