using Enemies;
using Sound;
using UnityEngine;

namespace Projectiles
{
    public class Arrow : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float speed = 25f;
        [SerializeField] private float maxRange = 40f;
        [SerializeField] private float tipOffset = 0.45f;
        [SerializeField] private float castRadius = 0.25f;
        [SerializeField] private LayerMask collisionMask;

        [Header("Damage")]
        [SerializeField] private int damage = 1;

        [Header("Stuck")]
        [SerializeField] private float stuckLifetime = 15f;
        [SerializeField] private Collider standingCollider;

        [Header("SFX")] [SerializeField] private AudioClip arrowHit;

        private Vector3 _direction;
        private float _distanceTraveled;
        private bool _launched;
        private bool _stuck;

        private void Awake()
        {
            if (standingCollider != null)
                standingCollider.enabled = false;
        }

        private void Update()
        {
            if (!_launched || _stuck)
                return;

            float step = speed * Time.deltaTime;

            if (CheckInitialOverlap())
                return;

            if (Physics.SphereCast(
                    transform.position,
                    castRadius,
                    _direction,
                    out RaycastHit hit,
                    step + tipOffset,
                    collisionMask,
                    QueryTriggerInteraction.Ignore))
            {
                HandleHit(hit);
                return;
            }

            transform.position += _direction * step;
            _distanceTraveled += step;

            if (_distanceTraveled >= maxRange)
                Destroy(gameObject);
        }

        private bool CheckInitialOverlap()
        {
            Collider[] initialHits = Physics.OverlapSphere(
                transform.position,
                castRadius,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (Collider hitCollider in initialHits)
            {
                EnemyHealth enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    StickToEnemy(hitCollider.transform, transform.position);
                    return true;
                }
            }

            return false;
        }

        private void HandleHit(RaycastHit hit)
        {
            EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                StickToEnemy(hit.collider.transform, hit.point);
                return;
            }

            AudioManager.Instance.PlaySfx(arrowHit);
            StickToEnvironment(hit.point);
        }

        private void StickToEnemy(Transform enemyTransform, Vector3 hitPoint)
        {
            _stuck = true;
            _launched = false;

            transform.position = hitPoint - _direction * tipOffset;
            transform.forward = _direction;

            transform.SetParent(enemyTransform, true);

            DisableAllColliders();

            Destroy(gameObject, stuckLifetime);
        }

        private void StickToEnvironment(Vector3 hitPoint)
        {
            _stuck = true;
            _launched = false;

            transform.position = hitPoint - _direction * tipOffset;
            transform.forward = _direction;

            if (standingCollider != null)
            {
                standingCollider.enabled = true;
                standingCollider.isTrigger = false;
            }

            enabled = false;
            Destroy(gameObject, stuckLifetime);
        }

        private void DisableAllColliders()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders)
                collider.enabled = false;
        }

        public void Launch(Vector3 direction)
        {
            _direction = direction.normalized;
            _distanceTraveled = 0f;
            _launched = true;
            _stuck = false;

            if (standingCollider != null)
                standingCollider.enabled = false;
        }
    }
}