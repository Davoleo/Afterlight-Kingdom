using UnityEngine;

namespace Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;
        private int currentHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0;

        private void Start()
        {
            ResetEnemy();
        }

        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;

            currentHealth -= damage;

            if (currentHealth <= 0)
                Die();
        }

        private void Die()
        {
            gameObject.SetActive(false);
        }

        public void ResetEnemy()
        {
            currentHealth = maxHealth;
        }
    }
}