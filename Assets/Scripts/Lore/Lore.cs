//Sasha Koroleva

using UnityEngine;

public class Lore : MonoBehaviour
{
    [SerializeField] private GameObject loreCanvas;
    [SerializeField] private GameObject openPromptUI;
    
    private bool _playerInRange = false;

    private void Awake()
    {
        if (openPromptUI != null) openPromptUI.SetActive(false);
        if (loreCanvas   != null) loreCanvas  .SetActive(false);

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            if (openPromptUI != null)
                openPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (openPromptUI != null)
                openPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (loreCanvas != null && loreCanvas.activeSelf)
            {
                CloseLore();
            }
            else if (_playerInRange && openPromptUI != null && openPromptUI.activeSelf)
            {
                openPromptUI.SetActive(false);
                loreCanvas  .SetActive(true);
            }
        }
    }

    public void CloseLore()
    {
        if (loreCanvas != null)
            loreCanvas.SetActive(false);

        if (_playerInRange && openPromptUI != null)
            openPromptUI.SetActive(true);
    }
    
    
}

