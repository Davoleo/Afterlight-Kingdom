using Player;
using UnityEngine;

namespace Enemies
{
    public static class EnemyPlayerDamage
    {
        public static bool TryDamage(
            Collider collider,
            int damage,
            Vector3 knockbackDirection,
            bool useKnockback,
            float knockbackDistance = 0f)
        {
            PlayerDamageFeedback damageFeedback =
                collider.GetComponentInParent<PlayerDamageFeedback>();

            if (damageFeedback != null)
            {
                return damageFeedback.TryTakeDamage(
                    damage,
                    knockbackDirection,
                    useKnockback,
                    knockbackDistance
                );
            }

            HealthManager health =
                collider.GetComponentInParent<HealthManager>();

            if (health != null)
            {
                health.TakeDamage(damage);
                return true;
            }

            return false;
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