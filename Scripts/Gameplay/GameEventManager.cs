using System;
using Player;
using UnityEngine;

namespace Gameplay
{
    public class GameEventManager : MonoBehaviour
    {

        private GameObject player;
        private CheckpointManager cpManager;
        private HealthManager health;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            cpManager = GetComponent<CheckpointManager>();
            health = player.GetComponent<HealthManager>();
        }

        private void Update()
        {
            if (player.transform.position.y < -80)
            {
                health.TakeDamage(HealthManager.MaxHealth);
            }
        }
    }
}