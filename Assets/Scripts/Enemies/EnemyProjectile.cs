using UnityEngine;

namespace Enemies
{
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifeTime = 5f;

        [Header("Damage")]
        [SerializeField] private int damage = 1;
        [SerializeField] private LayerMask playerLayer;

        private Vector3 direction;
        private bool launched;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
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

            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayer) != 0)
            {
                HitPlayer(other);
                return;
            }

            Destroy(gameObject);
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
    }
}