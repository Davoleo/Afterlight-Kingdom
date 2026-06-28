using System;
using Controllers;
using UnityEngine;

namespace Player
{
    public class HealthManager : MonoBehaviour
    {
        private MenuManager _menuManager;
        private int _health;
        private bool _isDead;

        public const int MaxHealth = 6;

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

        private void HandleDeath()
        {
            _isDead = true;

            if (_menuManager != null)
                _menuManager.ShowDeathScreen();
        }
    }
}