//Farzana Tanni

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public GameObject holdPoint;
    public static PlayerManager instance;

    void Awake()
    {
        Debug.Log("PlayerManager Awake called!");
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);

        }
    }

    private void OnDestroy()
    {
        instance = null;
    }
}