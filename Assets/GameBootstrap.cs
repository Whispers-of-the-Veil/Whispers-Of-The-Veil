//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        var chestIDs = new [] { "chestCabin", "chestHall", "chestKitchen", "chestBedroom" };
        foreach (var id in chestIDs)
            PlayerPrefs.DeleteKey("chest_open_" + id);
        PlayerPrefs.Save();
    }
}
