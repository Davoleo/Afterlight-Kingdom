using Player;
using UnityEngine;

namespace Enemies
{
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private float collisionRadius = 0.15f;

        [Header("Damage")]
        [SerializeField] private int damage = 1;
        [SerializeField] private LayerMask playerLayer;

        [Header("Environment")]
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private bool destroyOnEnvironmentHit = true;

        private Vector3 direction;
        private bool launched;
        private bool stopped;

        private void Start()
        {
            Destroy(gameObject, maxLifetime);
        }

        private void Update()
        {
            if (!launched || stopped)
                return;

            MoveProjectile();
        }

        public void Launch(Vector3 launchDirection)
        {
            direction = launchDirection.normalized;
            launched = true;
            stopped = false;

            Debug.Log("Enemy projectile launched.");
        }

        private void MoveProjectile()
        {
            float distance = speed * Time.deltaTime;

            bool hitEnvironment = Physics.SphereCast(
                transform.position,
                collisionRadius,
                direction,
                out RaycastHit hit,
                distance,
                environmentLayer,
                QueryTriggerInteraction.Ignore
            );

            if (hitEnvironment)
            {
                transform.position = hit.point - direction * collisionRadius;
                StopOnEnvironment();
                return;
            }

            transform.position += direction * distance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (stopped)
                return;

            if (IsInLayerMask(other.gameObject.layer, playerLayer))
            {
                HitPlayer(other);
                return;
            }

            if (IsInLayerMask(other.gameObject.layer, environmentLayer))
            {
                StopOnEnvironment();
            }
        }

        private void HitPlayer(Collider other)
        {
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

        private void StopOnEnvironment()
        {
            stopped = true;
            launched = false;

            Debug.Log("Enemy projectile stopped on environment.");

            if (destroyOnEnvironmentHit)
                Destroy(gameObject);
        }

        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, collisionRadius);
        }
    }
}