using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerStats : CharacterStats
    {
        private void Start()
        {
            GetReferences();
            InitVariables();
        }

        private void GetReferences()
        {
            
        }
        public override void CheckHealth()
        {
            base.CheckHealth();
        }
        public override void Die()
        {
            isDead = true;
            Destroy(gameObject);

        }
    }
}