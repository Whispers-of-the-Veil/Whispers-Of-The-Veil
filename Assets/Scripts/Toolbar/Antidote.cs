using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Antidote : InventoryItemBase
{
    public override string Name
    {
        get { return "Antidote"; }
    }

    public override void OnUse()
    {
        base.OnUse();
    }
    
}