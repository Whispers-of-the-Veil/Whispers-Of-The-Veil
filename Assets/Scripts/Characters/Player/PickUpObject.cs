// Sasha Koroleva

using Combat;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    [SerializeField] private ParticleSystem highlightParticles;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        highlightParticles.Play();
    }

    public void PickUp(Transform holdPoint)
    {
        // Move the object to the hold point and attach it to the parent
        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation;
        transform.parent = holdPoint;

        // Disable the BoxCollider2D so it doesn't interfere while held
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.isTrigger = true; // Set collider to trigger while held
        }

        MeleeWeapon weapon = GetComponent<MeleeWeapon>();
        if (weapon != null)
        {
            weapon.enabled = true;
        }

        // Disable Rigidbody2D physics while held
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics while held
        }
        
        highlightParticles.Clear();
        highlightParticles.Stop();
        highlightParticles.Clear();
    }

    public void Drop()
    {
        // Detach the object from the hold point
        transform.parent = null;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.isTrigger = true; // Keep collider as a trigger when dropped
        }

        MeleeWeapon weapon = GetComponent<MeleeWeapon>();
        if (weapon != null)
        {
            weapon.enabled = false;
        }

        // Re-enable Rigidbody2D physics after being dropped
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false; // Re-enable physics after drop
        }
        
        highlightParticles.Play();
    }
    public void SetSortingLayer(string layerName)
    {
        SpriteRenderer childRenderer = GetComponentInChildren<SpriteRenderer>();
        if (childRenderer != null)
        {
            childRenderer.sortingLayerName = layerName;
        }
    }

}
