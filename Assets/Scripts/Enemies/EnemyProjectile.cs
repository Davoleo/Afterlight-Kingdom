using Player;
using UnityEngine;

namespace Enemies
{
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float maxLifetime = 5f;

        [Header("Damage")]
        [SerializeField] private int damage = 1;
        [SerializeField] private LayerMask playerLayer;

        private Vector3 direction;
        private bool launched;

        private void Start()
        {
            Destroy(gameObject, maxLifetime);
        }

        private void Update()
        {
            if (!launched)
                return;

            transform.position += direction * speed * Time.deltaTime;
        }

        public void Launch(Vector3 launchDirection)
        {
            direction = launchDirection.normalized;
            launched = true;

            Debug.Log("Enemy projectile launched.");
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            PlayerDamageFeedback damageFeedback =
                other.GetComponentInParent<PlayerDamageFeedback>();

            if (damageFeedback != null)
            {
                damageFeedback.TryTakeDamage(damage, Vector3.zero, false);
                Debug.Log("Player hit by ranged projectile.");
            }
            else
            {
                HealthManager healthManager =
                    other.GetComponentInParent<HealthManager>();

                if (healthManager != null)
                {
                    healthManager.TakeDamage(damage);
                    Debug.LogWarning("PlayerDamageFeedback non trovato, usato HealthManager diretto.");
                }
            }

            Destroy(gameObject);
        }
    }
}