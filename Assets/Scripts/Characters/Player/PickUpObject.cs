//Sasha Koroleva

using UnityEngine;

public class PickUpObject : MonoBehaviour
{
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
            box.enabled = false;
        }
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 5; 
        }
    }
    
    public void Drop()
    {
        // Detach the object from the hold point
        transform.parent = null;
        

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.enabled = true;
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 0;
        }
    }
}

