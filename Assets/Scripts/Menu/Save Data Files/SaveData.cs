// Farzana Tanni

using System;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int slot;
    public string sceneName;
    public float playerX, playerY, playerZ;
    public string savedTime;

    public SaveData(int slot, string scene, Vector3 position)
    {
        this.slot = slot;
        this.sceneName = scene;
        this.playerX = position.x;
        this.playerY = position.y;
        this.playerZ = position.z;
        this.savedTime = System.DateTime.Now.ToString("g");
    }
}

