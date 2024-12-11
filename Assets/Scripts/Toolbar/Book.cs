using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Book : InventoryItemBase
{
    [SerializeField] private GameObject bookCanvas;
    private bool isBookOpen = false;
    public override string Name
    {
        get { return "Book"; }
    }

    public override void OnUse()
    {
        base.OnUse();
    }

    public void OpenBook()
    {
        isBookOpen = !isBookOpen;
        bookCanvas.SetActive(isBookOpen);
        
    }
    
}
