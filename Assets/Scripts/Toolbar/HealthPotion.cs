using UnityEngine;
using Characters.Player;

public class HealthPotion : InventoryItemBase
{
    public float healAmount = 1f;
    private Transform lastParent;
    private PlayerStats playerStats;

    public override string Name
    {
        get { return "Health Potion"; }
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
            playerStats.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}