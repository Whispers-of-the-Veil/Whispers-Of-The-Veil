using UnityEngine;
using Characters.Player;

public class InvisibilityPotion : InventoryItemBase
{
    public float duration = 5f;
    private Transform lastParent;
    private PlayerStats playerStats;

    public override string Name
    {
        get { return "Invisibility Potion"; }
    }
    private void Update()
    {
        if (transform.parent != null && transform.parent != lastParent)
        {
            lastParent = transform.parent;
            playerStats = transform.GetComponentInParent<PlayerStats>();
        }

        if (playerStats != null && Input.GetMouseButtonDown(0))
        {
            Debug.Log("Player consumed Invisibility Potion");

            playerStats.SetInvisibility(true, duration);
            Destroy(gameObject);
        }
    }
}