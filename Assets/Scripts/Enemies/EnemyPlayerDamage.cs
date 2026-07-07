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

            PlayerDamageFeedback damageFeedback =
                hitCollider.GetComponentInParent<PlayerDamageFeedback>();

            if (damageFeedback != null)
            {
                if (knockbackDistance > 0f)
                {
                    return damageFeedback.TryTakeDamage(
                        damage,
                        knockbackDirection,
                        useKnockback,
                        knockbackDistance
                    );
                }

                return damageFeedback.TryTakeDamage(
                    damage,
                    knockbackDirection,
                    useKnockback
                );
            }

            HealthManager health =
                hitCollider.GetComponentInParent<HealthManager>();

            if (health == null)
                return false;

            health.TakeDamage(damage);
            return true;
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