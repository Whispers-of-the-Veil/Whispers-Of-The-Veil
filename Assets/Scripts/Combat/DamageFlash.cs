//Owen Ingram

using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public float flashDuration = 0.1f;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found in children of " + gameObject.name);
        }
        else
        {
            Debug.Log("SpriteRenderer found: " + spriteRenderer.gameObject.name);
            originalColor = spriteRenderer.color; 
        }
    }

    public void FlashRed()
    {
        if (spriteRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}