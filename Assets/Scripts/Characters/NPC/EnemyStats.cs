using System.Collections;
using System.Collections.Generic;
using Audio.SFX;
using UnityEngine;
using UnityEngine.SceneManagement; 
using Characters.Player;

namespace Characters.NPC
{
    public class EnemyStats : MonoBehaviour
    {
        [Header("Animation")]
        private Animator animator;
        
        [Header("Victory")]
        [SerializeField] private bool   isMainBoss = false;
        [SerializeField] private string victorySceneName;
        
        [Header("Audio")]
        [SerializeField] AudioClip deathSfx;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
        [SerializeField] private float health;
        [SerializeField] private float damage = 1f;
        [SerializeField] private bool canAttack = true;
        public float attackSpeed = 1.5f;
        public bool isDead;
        
        public CombatExpert combatExpert {
            get => CombatExpert.instance;
        }

        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
            isDead = false;
        }

        public void DealDamage(PlayerStats playerStats)
        {
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                playerStats.UpdateHealth();
                Debug.Log("Enemy attacked the player!");
            }
        }

        public void TakeDamage(float damageAmount)
        {
            health -= damageAmount;
            Debug.Log($"Enemy took {damageAmount} damage. Remaining health: {health}");

            if (health <= 0)
            {
                animator.SetTrigger("Dead");
                StartCoroutine(DelayDeath(0.75f));
            }
        }

        IEnumerator DelayDeath(float time) {
            combatExpert.ReportCombat(false);
            yield return new WaitForSeconds(time);
            Die();
        }

        public void Die()
        {
            sfxManager.PlaySFX(deathSfx, transform, 1);
            
            isDead = true;
            if (isMainBoss && !string.IsNullOrEmpty(victorySceneName))
            {
                SceneManager.LoadScene(victorySceneName);
            }
            Debug.Log("Enemy defeated");
            Destroy(gameObject);
        }
    }
}