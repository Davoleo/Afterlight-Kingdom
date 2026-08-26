using UnityEngine;

namespace Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        private int currentHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0;

        private EnemySoundFXs _sfx;

        private void Start()
        {
            _sfx = GetComponent<EnemySoundFXs>();
            ResetEnemy();
        }

        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;

            currentHealth -= damage;

            if (currentHealth <= 0)
                Die();
            else
                _sfx.OnEnemyHurt();
        }

        private void Die()
        {
            _sfx.OnEnemyDeath();
            gameObject.SetActive(false);
        }

        public void ResetEnemy()
        {
            currentHealth = maxHealth;
        }
    }
}