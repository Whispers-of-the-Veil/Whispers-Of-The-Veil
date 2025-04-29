//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;    
    public float floatAmount = 10f;  

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = originalPosition + new Vector3(0, newY, 0);
    }
}

