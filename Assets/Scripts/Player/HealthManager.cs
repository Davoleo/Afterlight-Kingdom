using System;
using Controllers;
using Core;
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
        private PlayerSoundFXs _sfx;
        private PlayerDamageFeedback _damageFeedback;

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
            _sfx = GetComponent<PlayerSoundFXs>();
            _damageFeedback = GetComponent<PlayerDamageFeedback>();

            if (gameManager)
                _menuManager = gameManager.GetComponent<MenuManager>();
        }

        public void TakeDamage(int damage)
        {
            TakeDamage(damage, Vector3.zero, false);
        }

        public bool TakeDamage(int damage, Vector3 knockbackDirection, bool applyKnockback, float customKnockbackDistance = -1f)
        {
            if (_isDead)
                return false;

            if (_damageFeedback != null && _damageFeedback.IsInvincible)
                return false;

            Health = Math.Max(Health - damage, 0);
            _sfx.OnPlayerHurt();

            if (_isDead)
            {
                _damageFeedback.StopAllDamageFeedback();
                return true;
            }

            _damageFeedback.PlayDamageFeedback(knockbackDirection, applyKnockback, customKnockbackDistance);

            return true;
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
            _characterController.motor.BaseVelocity = Vector3.zero;

            // Let systems holding transient state (animator, bow visuals...) reset themselves
            GameStateManager.NotifyRespawned();
        }

        private void HandleDeath()
        {
            _isDead = true;

            if (_menuManager)
                _menuManager.ShowDeathScreen();
        }
    }
}