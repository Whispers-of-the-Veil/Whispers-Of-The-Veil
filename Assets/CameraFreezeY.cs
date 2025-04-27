using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFreezeY : MonoBehaviour
{
    private float fixedY;

    void Start()
    {
        // Remember the camera’s starting world-Y
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        // After everything moves, snap Y back to the fixed value
        Vector3 p = transform.position;
        p.y = fixedY;
        transform.position = p;
    }
}
