using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Characters.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] public float health = 3f;
        [SerializeField] public int maxHealth = 3;
        [SerializeField] private bool isDead;
        [SerializeField] Image[] hearts;
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite halfHeart;
        [SerializeField] private Sprite emptyHeart;
        private Vector3 defaultHeartScale = Vector3.one;
        private float damageEffectScale = 1.2f;
        private float scaleDuration = 0.1f;
        private SpriteRenderer[] spriteRenderers;


        private void Start()
        {
            if (health <= 0f || health > maxHealth) // Only reset if uninitialized
            {
                //health = maxHealth;
            }
            FindHearts();
            UpdateHealth();
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

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
            StartCoroutine(DelayedUIUpdate());
        }
        
        private IEnumerator DelayedUIUpdate()
        {
            yield return null;
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
                    Debug.Log("Hearts found: " + hearts.Length);
                    UpdateHealth(); // Force refresh immediately
                }
            }
        }



        private void Update()
        {
        }

        public void UpdateHealth()
        {
            if (hearts == null || hearts.Length == 0)
            {
                return;
            }

            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null)
                {
                    continue;
                }
                
                if (health >= i + 1)
                    hearts[i].sprite = fullHeart;
                else if (health >= i + 0.5f)
                    hearts[i].sprite = halfHeart;
                else
                    hearts[i].sprite = emptyHeart;
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
        
        public bool isInvisible { get; private set; }

        public void SetInvisibility(bool state, float duration)
        {
            if (isInvisible) return;
            isInvisible = true;
            SetAlphaForAll(0.4f);
            StartCoroutine(ResetInvisibility(duration));
        }

        private IEnumerator ResetInvisibility(float duration)
        {
            yield return new WaitForSeconds(duration);
            isInvisible = false;
            SetAlphaForAll(1f);
        }

        private void SetAlphaForAll(float alpha)
        {
            if (spriteRenderers == null) return;

            foreach (var renderer in spriteRenderers)
            {
                if (renderer != null)
                {
                    Color c = renderer.color;
                    c.a = alpha;
                    renderer.color = c;
                }
            }
        }
        
        public void Heal(float amount)
        {
            if (isDead) return;

            health += amount;
            health = Mathf.Clamp(health, 0f, maxHealth);
            Debug.Log($"Player healed {amount}. Current health: {health}");
            UpdateHealth();
        }
        //CheckpointManager.Instance.RespawnPlayer(gameObject);
        //health = maxHealth;
        //UpdateHealth();
        public void Die()
        {
            isDead = true;

            if (menu.DeathScreenUI.Instance != null)
            {
                menu.DeathScreenUI.Instance.ShowDeathScreen();
            }
        }
    }
}