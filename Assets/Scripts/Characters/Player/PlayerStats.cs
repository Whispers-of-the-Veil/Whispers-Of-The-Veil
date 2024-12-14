//Owen Ingram

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HealthHUD;

namespace Characters.Player
{
    public class PlayerStats : CharacterStats
    {
        private HUDHealth hud;
        
        private void Start()
        {
            GetReferences();
            InitVariables();
        }

        private void GetReferences()
        {
            hud = GetComponent<HUDHealth>();
        }

        public override void CheckHealth()
        {
            base.CheckHealth();
            hud.UpdateHealth(health, maxHealth);
        }

        public override void Die()
        {
            isDead = true;
            CheckpointManager.Instance.RespawnPlayer(gameObject);
            health = 100;
            Debug.Log("Player died, respawning");
        }
    }
}