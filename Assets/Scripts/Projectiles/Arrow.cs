using Enemies;
using UnityEngine;

namespace Projectiles
{
    public class Arrow : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float speed = 25f;
        [SerializeField] private float maxRange = 40f;
        [SerializeField] private float tipOffset = 0.45f;
        [SerializeField] private LayerMask collisionMask;

        [Header("Damage")]
        [SerializeField] private int damage = 1;
        [SerializeField] private bool destroyOnEnemyHit = true;

        [Header("Stuck")]
        [SerializeField] private float stuckLifetime = 15f;
        [SerializeField] private Collider standingCollider;

        private Vector3 _direction;
        private float _distanceTraveled;
        private bool _launched;

        private void Update()
        {
            if (!_launched)
                return;

            float step = speed * Time.deltaTime;

            if (Physics.Raycast(transform.position, _direction, out RaycastHit hit, step + tipOffset, collisionMask))
            {
                EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log("Arrow hit enemy.");

                    if (destroyOnEnemyHit)
                        Destroy(gameObject);
                    else
                        Stick(hit.point);

                    return;
                }

                Stick(hit.point);
                return;
            }

            transform.position += _direction * step;
            _distanceTraveled += step;

            if (_distanceTraveled >= maxRange)
                Destroy(gameObject);
        }

        private void Stick(Vector3 hitPoint)
        {
            transform.position = hitPoint - _direction * tipOffset;
            transform.forward = _direction;

            if (standingCollider != null)
                standingCollider.enabled = true;

            enabled = false;
            Destroy(gameObject, stuckLifetime);
        }

        public void Launch(Vector3 direction)
        {
            _direction = direction.normalized;
            _launched = true;

            if (standingCollider != null)
                standingCollider.enabled = false;
        }
    }
}