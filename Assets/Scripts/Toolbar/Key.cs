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
    
    public static Key instance;

    public override void OnUse()
    {
        base.OnUse();
    }
    
}
