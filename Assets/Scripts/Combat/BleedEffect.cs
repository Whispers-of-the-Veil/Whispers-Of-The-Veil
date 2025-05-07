//Owen Ingram

using System.Collections;
using UnityEngine;
using Characters.NPC;

namespace Combat
{
    public class BleedEffect : MonoBehaviour
    {
        public int bleedDamage = 2;
        public float duration = 3f;
        public float tickInterval = 1f;

        private Coroutine bleedCoroutine;

        public void ApplyBleed()
        {
            if (bleedCoroutine != null)
            {
                StopCoroutine(bleedCoroutine);
            }
            bleedCoroutine = StartCoroutine(BleedDamage());
        }

        private IEnumerator BleedDamage()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += tickInterval;

                Enemy enemy = GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(bleedDamage);
                }

                yield return new WaitForSeconds(tickInterval);
            }
        }
    }
}