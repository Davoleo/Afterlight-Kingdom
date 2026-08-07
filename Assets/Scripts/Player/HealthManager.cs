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
        private PlayerCharacterController _characterController;

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
            _characterController = GetComponent<PlayerCharacterController>();

            if (gameManager)
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
            //flush the inputs
            _characterController.ResetInputs();
            // avoid character to keep momentum after respawn 
            _characterController.motor.BaseVelocity  = Vector3.zero;
        }

        private void HandleDeath()
        {
            _isDead = true;

            if (_menuManager)
                _menuManager.ShowDeathScreen();
        }
    }
}