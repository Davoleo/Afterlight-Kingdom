using UnityEngine;

namespace Enemies
{
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifeTime = 5f;
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
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            if (!launched || stopped)
                return;

            MoveProjectile();
        }

        public void Launch(Vector3 launchDirection)
        {
            //invalid direction
            if (launchDirection.sqrMagnitude < 0.01f)
            {
                return;
            }

            direction = launchDirection.normalized;
            launched = true;
            stopped = false;

            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void MoveProjectile()
        {
            float distance = speed * Time.deltaTime;
            int collisionMask = playerLayer.value | environmentLayer.value;

            bool hasHit = Physics.SphereCast(
                transform.position,
                collisionRadius,
                direction,
                out RaycastHit hit,
                distance,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );

            if (hasHit)
            {
                transform.position = hit.point - direction * collisionRadius;
                HandleCollision(hit.collider);
                return;
            }

            transform.position += direction * distance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (stopped)
                return;

            HandleCollision(other);
        }

        private void HandleCollision(Collider other)
        {
            if (other == null)
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
            EnemyPlayerDamage.TryDamage(
                other,
                damage,
                Vector3.zero,
                false
            );

            Destroy(gameObject);
        }

        private void StopOnEnvironment()
        {
            stopped = true;
            launched = false;

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