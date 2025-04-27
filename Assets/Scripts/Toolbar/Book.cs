//Sasha Koroleva

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Book : InventoryItemBase
{
    [SerializeField] private GameObject bookCanvas;
    [SerializeField] private GameObject openPromptUI;
    
    private bool isBookOpen = false;
    private bool hasBeenPickedUp = false;

    public bool IsBookOpen => isBookOpen;

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
    
    public void OnPickedUp()
    {
        hasBeenPickedUp = true;
        if (openPromptUI != null && !isBookOpen)
            openPromptUI.SetActive(true);
        
    }
    public void OnDropped()
    {
        hasBeenPickedUp = false;
        if (openPromptUI != null)
            openPromptUI.SetActive(false);
        
    }
    
    
    
}
