//Sasha Koroleva

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Key : InventoryItemBase
{
    public override string Name
    {
        get { return "Key"; }
    }
    private static bool _hasBeenPickedUp = false;
    public static Key instance { get; private set; }
    
    
    public override void OnUse()
    {
        base.OnUse();
    }
    private void Awake()
    {
        if (_hasBeenPickedUp)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void OnPickedUp()
    {
        _hasBeenPickedUp = true;

    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
    
}
