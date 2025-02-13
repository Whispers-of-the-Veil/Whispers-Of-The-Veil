//Owen Ingram

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.Player
{
    public class PlayerStats : CharacterStats
    {
        [SerializeField] private Image[] hearts;

        private void Start()
        {
            InitVariables();
            UpdateHealth();
        }

        public void UpdateHealth()
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (i < health)
                {
                    //hearts[i].color = Color.red;
                }
                else
                {
                    hearts[i].color = Color.black;

                }
            }
        }
        

        public override void Die()
        {
            isDead = true;
            CheckpointManager.Instance.RespawnPlayer(gameObject);
            health = 3;
            Debug.Log("Player died, respawning");
        }
    }
}