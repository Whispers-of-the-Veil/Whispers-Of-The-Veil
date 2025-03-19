//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class sword : InventoryItemBase
{
    public override string Name
    {
        get { return "sword"; }
    }

    public override void OnUse()
    {
        base.OnUse();
    }
}
