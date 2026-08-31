using System.Linq;
using Enemies;
using KinematicCharacterController;
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

        private void Start()
        {
            if (standingCollider == null)
                standingCollider = GetComponent<Collider>();
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

                if (enemyHealth is not null)
                {
                    StickToEnemy(hitCollider.transform, transform.position);
                    enemyHealth.TakeDamage(damage);
                    return true;
                }
            }

            return false;
        }

        private void HandleHit(RaycastHit hit)
        {
            EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth is not null)
            {
                StickToEnemy(hit.collider.transform, hit.point);
                enemyHealth.TakeDamage(damage);
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

            Destroy(gameObject, stuckLifetime);
        }

        private void StickToEnvironment(Vector3 hitPoint)
        {
            _stuck = true;
            _launched = false;

            transform.position = hitPoint - _direction * tipOffset;
            transform.forward = _direction;

            standingCollider.enabled = true;
            standingCollider.isTrigger = false;

            enabled = false;
            Destroy(gameObject, stuckLifetime);
        }

        public void Launch(Vector3 direction)
        {
            _direction = direction.normalized;
            _distanceTraveled = 0f;
            _launched = true;
            _stuck = false;

            standingCollider.enabled = false;
        }
    }
}