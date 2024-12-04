using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    public void PickUp(Transform holdPoint)
    {
        Rigidbody objRb = gameObject.GetComponent<Rigidbody>();
        if (objRb != null) {
            objRb.isKinematic = true;  // Disable physics on the object while held
        }
        gameObject.transform.position = holdPoint.position;
        gameObject.transform.rotation = holdPoint.rotation;  // Match rotation to holdPoint
        gameObject.transform.parent = holdPoint;
        
        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        box.enabled = false;
        
    }
    
    public void Drop()
    {
        Rigidbody objRb = gameObject.GetComponent<Rigidbody>();
        if (objRb != null) {
            objRb.isKinematic = false;  // Re-enable physics on the object
        }
        gameObject.transform.parent = null;
        
        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        box.enabled = true;

    }
}
