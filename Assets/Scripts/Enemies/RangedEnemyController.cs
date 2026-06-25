using UnityEngine;

namespace Enemies
{
    public class RangedEnemyController : EnemyController
    {
        private enum RangedState
        {
            Sleeping,
            Patrolling,
            Shooting
        }

        [Header("Ranged References")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject projectilePrefab;

        [Header("Shooting")]
        [SerializeField] private float shootCooldown = 1.2f;
        [SerializeField] private float shootWindup = 0.25f;

        private RangedState currentState;
        private bool isPreparingShot;
        private float shotStartTime;
        private float lastShotTime;

        public override void Activate()
        {
            if (currentState == RangedState.Sleeping)
                ChangeState(RangedState.Patrolling);
        }

        protected override void SetInitialState()
        {
            ChangeState(startsActive ? RangedState.Patrolling : RangedState.Sleeping);
        }

        protected override void UpdateEnemy()
        {
            UpdateState();
            UpdateShot();
        }

        protected override void ResetSpecificState()
        {
            isPreparingShot = false;
            lastShotTime = Time.time;
        }

        protected override void SetStateAfterReset()
        {
            ChangeState(startsActive ? RangedState.Patrolling : RangedState.Sleeping);
        }

        private void UpdateState()
        {
            if (currentState == RangedState.Sleeping)
                return;

            if (IsPlayerDead())
                return;

            float distanceFromPlayer = DistanceFromPlayer();

            if (distanceFromPlayer <= target.DetectionRange)
            {
                ChangeState(RangedState.Shooting);
                TryShoot();
                return;
            }

            if (currentState == RangedState.Shooting && distanceFromPlayer >= target.LosePlayerRange)
            {
                isPreparingShot = false;
                ChangeState(RangedState.Patrolling);
            }
        }

        private void TryShoot()
        {
            if (isPreparingShot)
                return;

            if (Time.time < lastShotTime + shootCooldown)
                return;

            isPreparingShot = true;
            shotStartTime = Time.time;
        }

        private void UpdateShot()
        {
            if (!isPreparingShot)
                return;

            if (currentState != RangedState.Shooting)
            {
                isPreparingShot = false;
                return;
            }

            if (Time.time < shotStartTime + shootWindup)
                return;

            isPreparingShot = false;
            lastShotTime = Time.time;

            Shoot();
        }

        private void Shoot()
        {
            if (shootPoint == null || projectilePrefab == null || PlayerTransform() == null)
                return;

            Vector3 shootDirection = GetPlayerAimPosition(1f) - shootPoint.position;

            if (shootDirection.sqrMagnitude < 0.01f)
                return;

            shootDirection.Normalize();

            GameObject projectile = Instantiate(
                projectilePrefab,
                shootPoint.position,
                Quaternion.LookRotation(shootDirection)
            );

            EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();

            if (enemyProjectile != null)
                enemyProjectile.Launch(shootDirection);
        }

        private void ChangeState(RangedState newState)
        {
            currentState = newState;
        }

        protected override void UpdateMovementDirection()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            switch (currentState)
            {
                case RangedState.Sleeping:
                    break;

                case RangedState.Patrolling:
                    MovementDirection = GetPatrolDirection();
                    LookDirection = MovementDirection;
                    break;

                case RangedState.Shooting:
                    MovementDirection = Vector3.zero;
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (shootPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
            }
        }
    }
}