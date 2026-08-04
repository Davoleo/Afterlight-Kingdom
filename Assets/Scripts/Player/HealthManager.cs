using System;
using Controllers;
using Projectiles;
using UnityEngine;

namespace Player
{
    public class HealthManager : MonoBehaviour
    {
        private MenuManager _menuManager;
        private int _health;
        private bool _isDead;

        public const int MaxHealth = 6;

        private ArrowLauncher _arrowLauncher;

        public int Health
        {
            get => _health;
            private set
            {
                _health = value;

                if (_health <= 0 && !_isDead)
                    HandleDeath();
            }
        }

        private void Start()
        {
            Health = MaxHealth;

            GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
            _arrowLauncher = GetComponent<ArrowLauncher>();

            if (gameManager != null)
                _menuManager = gameManager.GetComponent<MenuManager>();
        }

        public void TakeDamage(int damage)
        {
            if (_isDead) return;

            Health = Math.Max(Health - damage, 0);
        }

        public void Heal(int heal)
        {
            if (_isDead)
                return;

            Health = Math.Min(Health + heal, MaxHealth);
        }

        /// reset all params after death
        public void ResetAfterRespawn()
        {
            _arrowLauncher.ClearAllArrows();
            _isDead = false;
            Health = MaxHealth;
        }

        private void HandleDeath()
        {
            _isDead = true;

            if (_menuManager != null)
                _menuManager.ShowDeathScreen();
        }
    }
}