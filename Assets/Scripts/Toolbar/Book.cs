using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Book : InventoryItemBase
{
    public override string Name
    {
        get { return "Book"; }
    }

    public override void OnUse()
    {
        base.OnUse();
    }
    
}
