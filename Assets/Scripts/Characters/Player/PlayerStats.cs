using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Characters.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float health = 3f;
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private bool isDead;
        [SerializeField] Image[] hearts;
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite halfHeart;
        [SerializeField] private Sprite emptyHeart;
        private Vector3 defaultHeartScale = Vector3.one;
        private float damageEffectScale = 1.2f;
        private float scaleDuration = 0.1f;

        private void Start()
        {
            health = maxHealth;
            FindHearts();
            UpdateHealth();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindHearts();
            UpdateHealth();
        }

        private void FindHearts()
        {
            GameObject hud = GameObject.FindWithTag("HUD");
            if (hud != null)
            {
                Transform healthCanvas = hud.transform.Find("HealthCanvas");
                if (healthCanvas != null)
                {
                    hearts = healthCanvas.GetComponentsInChildren<Image>();
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                TakeDamage(0.5f);
            }
        }

        public void UpdateHealth()
        {
            if (hearts == null) return;

            for (int i = 0; i < hearts.Length; i++)
            {
                if (health >= i + 1)
                {
                    hearts[i].sprite = fullHeart;
                }
                else if (health >= i + 0.5f)
                {
                    hearts[i].sprite = halfHeart;
                }
                else
                {
                    hearts[i].sprite = emptyHeart;
                }
            }
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0f, maxHealth);
            Debug.Log($"Player took {damage} damage. Remaining health: {health}");

            UpdateHealth();
            StartCoroutine(ScaleHeartEffect());

            if (health <= 0)
            {
                Die();
            }
        }

        private Dictionary<int, Coroutine> heartAnimations = new Dictionary<int, Coroutine>();

        private IEnumerator ScaleHeartEffect()
        {
            if (hearts == null || health < 0) yield break;
            int damagedHeartIndex = Mathf.FloorToInt(health);
            if (damagedHeartIndex < hearts.Length)
            {
                Transform heartTransform = hearts[damagedHeartIndex].transform;
                
                if (heartAnimations.ContainsKey(damagedHeartIndex) && heartAnimations[damagedHeartIndex] != null)
                {
                    StopCoroutine(heartAnimations[damagedHeartIndex]);
                }
                Coroutine newAnimation = StartCoroutine(AnimateHeart(heartTransform, damagedHeartIndex));
                heartAnimations[damagedHeartIndex] = newAnimation;
            }
        }
        
        private IEnumerator AnimateHeart(Transform heart, int index)
        {
            Vector3 targetScale = defaultHeartScale * damageEffectScale;
            Vector3 originalScale = defaultHeartScale;
            float elapsedTime = 0f;

            while (elapsedTime < scaleDuration)
            {
                heart.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / scaleDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            heart.localScale = targetScale;
            yield return new WaitForSeconds(0.1f);
            elapsedTime = 0f;
            
            while (elapsedTime < scaleDuration)
            {
                heart.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / scaleDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            heart.localScale = originalScale;
            heart.localScale = defaultHeartScale;
            
            if (heartAnimations.ContainsKey(index))
            {
                heartAnimations[index] = null;
            }
        }
        
        public void Die()
        {
            isDead = true;
            CheckpointManager.Instance.RespawnPlayer(gameObject);
            health = maxHealth;
            UpdateHealth();
            Debug.Log("Player died, respawning");
        }
    }
}