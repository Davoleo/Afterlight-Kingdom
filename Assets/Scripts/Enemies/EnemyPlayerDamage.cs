using Player;
using UnityEngine;

namespace Enemies
{
    public static class EnemyPlayerDamage
    {
        public static bool TryDamage(
            Collider hitCollider,
            int damage,
            Vector3 knockbackDirection,
            bool useKnockback,
            float knockbackDistance = 0f)
        {
            if (hitCollider == null)
                return false;

            HealthManager health =
                hitCollider.GetComponentInParent<HealthManager>();

            if (health == null)
                return false;

            return health.TakeDamage(
                damage,
                knockbackDirection,
                useKnockback,
                knockbackDistance
            );
        }

        public static bool TryDamageFirstInSphere(
            Vector3 position,
            float radius,
            LayerMask playerLayer,
            int damage,
            Vector3 knockbackDirection,
            bool useKnockback,
            float knockbackDistance = 0f)
        {
            Collider[] hitColliders = Physics.OverlapSphere(
                position,
                radius,
                playerLayer
            );

            foreach (Collider hitCollider in hitColliders)
            {
                bool damaged = TryDamage(
                    hitCollider,
                    damage,
                    knockbackDirection,
                    useKnockback,
                    knockbackDistance
                );

                if (damaged)
                    return true;
            }

            return false;
        }
    }
}