using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HealthHUD
{

    public class HUDHealth : MonoBehaviour
    {
        [SerializeField] HealthBar healthBar;

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            healthBar.SetValues(currentHealth, maxHealth);
        }
    }
}