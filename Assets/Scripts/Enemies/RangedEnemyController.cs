using UnityEngine;

namespace Enemies
{
    public class RangedEnemyController : BaseEnemyController
    {
        private enum EnemyState
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
        [SerializeField] private float projectileSpawnOffset = 0.4f;

        private EnemyState currentState;
        private bool isPreparingShot;
        private float shotStartTime;
        private float lastShotTime;

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead())
                return;

            UpdateState();
            UpdateMovementDirection();
            UpdateShot();

            MoveAndRotate(Time.deltaTime);
        }

        public override void Activate()
        {
            if (currentState == EnemyState.Sleeping)
                ChangeState(EnemyState.Patrolling);
        }

        protected override void SetInitialState()
        {
            ChangeState(startsActive ? EnemyState.Patrolling : EnemyState.Sleeping);
        }

        protected override void OnResetToSpawn()
        {
            isPreparingShot = false;
            lastShotTime = Time.time;
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping)
                return;

            if (IsPlayerInsideDetection())
            {
                if (currentState != EnemyState.Shooting)
                    ChangeState(EnemyState.Shooting);

                TryShoot();
                return;
            }

            if (currentState == EnemyState.Shooting && IsPlayerOutsideLoseRange())
            {
                ChangeState(EnemyState.Patrolling);
                isPreparingShot = false;
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

            if (currentState != EnemyState.Shooting)
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
            Vector3 targetPosition = GetPlayerAimPosition(1f);
            Vector3 shootDirection = targetPosition - shootPoint.position;

            //player troppo vicino al nemico, TODO: gestire il movimento all'indietro del nemico per sparare
            if (shootDirection.sqrMagnitude < 0.01f)
                return;

            shootDirection.Normalize();

            Vector3 spawnPosition = shootPoint.position + shootDirection * projectileSpawnOffset;

            GameObject projectile = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(shootDirection)
            );

            projectile.SetActive(true);
            projectile.transform.SetParent(null);

            EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();

            if (enemyProjectile == null)
            {
                Destroy(projectile);
                return;
            }

            enemyProjectile.Launch(shootDirection);
        }

        private void ChangeState(EnemyState newState)
        {
            currentState = newState;
        }

        private void UpdateMovementDirection()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            switch (currentState)
            {
                case EnemyState.Sleeping:
                    break;

                case EnemyState.Patrolling:
                    MovementDirection = GetPatrolDirection();
                    LookDirection = MovementDirection;
                    break;

                case EnemyState.Shooting:
                    MovementDirection = Vector3.zero;
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        //draw range circles
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(shootPoint.position, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(shootPoint.position, shootPoint.position + shootPoint.forward * 1.5f);
        }
    }
}