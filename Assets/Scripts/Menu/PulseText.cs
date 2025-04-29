//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseText : MonoBehaviour
{
    public float pulseSpeed = 1f;   
    public float pulseAmount = 0.05f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * scale;
    }
}
