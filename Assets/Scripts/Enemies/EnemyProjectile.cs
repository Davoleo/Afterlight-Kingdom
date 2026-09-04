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

        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float pulseAmount = 0.08f;

        private Vector3 direction;
        private bool launched;
        private bool stopped;

        private Vector3 initialScale;

        private void Start()
        {
            initialScale = transform.localScale;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            UpdateVisualAnimation();

            if (!launched || stopped)
                return;

            MoveProjectile();
        }

        public void Launch(Vector3 launchDirection)
        {
            direction = launchDirection.normalized;
            launched = true;
            stopped = false;

            transform.rotation = Quaternion.LookRotation(direction);
        }

        private void MoveProjectile()
        {
            float distance = speed * Time.deltaTime;
            int collisionMask = playerLayer.value | environmentLayer.value;

            //added: checks if the projectile is already overlapping the player
            //when it starts very close to them
            Collider[] playerOverlaps = Physics.OverlapSphere(
                transform.position,
                collisionRadius,
                playerLayer,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider overlap in playerOverlaps)
            {
                HitPlayer(overlap);
                return;
            }

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

        private void UpdateVisualAnimation()
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);

            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = initialScale * pulse;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (stopped)
                return;

            HandleCollision(other);
        }

        private void HandleCollision(Collider other)
        {
            if (IsInLayerMask(other.gameObject.layer, playerLayer))
            {
                HitPlayer(other);
                return;
            }

            //if the projectile hits any other scene collider, destroy it
            StopOnEnvironment();
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
        public static void RemoveAllProjectiles()
        {
            EnemyProjectile[] projectiles = FindObjectsByType<EnemyProjectile>(FindObjectsSortMode.None);

            foreach (EnemyProjectile projectile in projectiles)
            {
                projectile.gameObject.SetActive(false);
                Destroy(projectile.gameObject);
            }
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