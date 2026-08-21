using UnityEngine;

namespace Enemies
{
    public class RangedEnemyController : BaseEnemyController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            Advancing,
            Shooting,
            PostShot,
            Retreating,
            Waiting
        }

        [Header("Ranged References")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject projectilePrefab;

        [Header("Ranged Movement")]
        [SerializeField] private float advanceSpeed = 2f;
        [SerializeField] private float retreatSpeed = 3f;
        [SerializeField] private float minimumShootingDistance = 3f;
        [SerializeField] private float desiredShootingDistance = 5f;
        [SerializeField] private float retreatDistance = 2.5f;
        [SerializeField] private float retreatNavMeshSampleRadius = 2f;

        [Header("Shooting")]
        [SerializeField] private float shootCooldown = 1.2f;
        [SerializeField] private float shootWindup = 0.25f;
        [SerializeField] private float projectileSpawnOffset = 0.4f;
        [SerializeField] private float postShotDelay = 0.5f;
        [SerializeField] private float playerChestAimOffset = -0.2f;

        [Header("Lost Player")]
        [SerializeField] private float lostPlayerIdleDuration = 1f;

        [Header("Return To Position")]
        [SerializeField] private float returnPositionTolerance = 0.15f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private EnemyState currentState;

        private bool isPreparingShot;
        private float shotStartTime;
        private float lastShotTime;
        private float stateStartTime;

        private Vector3 retreatDestination;
        private bool hasRetreatDestination;

        private Vector3 originalPosition;
        private Vector3 originalForward;

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead())
                return;

            UpdateState();
            UpdateMovementDirection();
            UpdateShot();
            UpdateAnimation();

            MoveAndRotate(Time.deltaTime);
        }

        public override void Activate()
        {
            if (currentState == EnemyState.Sleeping)
                ChangeState(EnemyState.Patrolling);
        }

        protected override void SetInitialState()
        {
            originalPosition = transform.position;
            originalForward = transform.forward;
            originalForward.y = 0f;

            if (originalForward.sqrMagnitude > 0.01f)
                originalForward.Normalize();

            ChangeState(startsActive ? EnemyState.Patrolling : EnemyState.Sleeping);
        }

        protected override void OnResetToSpawn()
        {
            isPreparingShot = false;
            shotStartTime = 0f;
            lastShotTime = Time.time;

            retreatDestination = Vector3.zero;
            hasRetreatDestination = false;

            if (animator == null)
                return;

            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsRetreating", false);
            animator.ResetTrigger("Shoot");
        }

        protected override float GetCurrentSpeed()
        {
            if (currentState == EnemyState.Retreating)
                return retreatSpeed;

            if (currentState == EnemyState.Advancing)
                return advanceSpeed;

            return patrolSpeed;
        }

        private float GetHorizontalDistanceFromPlayer()
        {
            if (!HasPlayer())
                return Mathf.Infinity;

            Vector3 playerPosition = Target.Player.position;
            Vector3 enemyPosition = transform.position;

            playerPosition.y = 0f;
            enemyPosition.y = 0f;

            return Vector3.Distance(enemyPosition, playerPosition);
        }

        private bool IsAtOriginalPosition()
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = originalPosition;

            currentPosition.y = 0f;
            targetPosition.y = 0f;

            return Vector3.Distance(currentPosition, targetPosition) <= returnPositionTolerance;
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping)
                return;

            /*
             * Once the casting animation has started,
             * the wizard must complete the shot.
             *
             * Distance changes, retreat and detection loss
             * cannot interrupt the cast.
             */
            if (currentState == EnemyState.Shooting && isPreparingShot)
                return;

            float distanceFromPlayer = GetHorizontalDistanceFromPlayer();

            if (currentState == EnemyState.PostShot)
            {
                if (Time.time < stateStartTime + postShotDelay)
                    return;

                if (!IsPlayerInsideDetection())
                {
                    ChangeState(EnemyState.Waiting);
                    return;
                }

                ChooseCombatState(distanceFromPlayer);
                return;
            }

            if (currentState == EnemyState.Waiting)
            {
                if (IsPlayerInsideDetection())
                {
                    ChooseCombatState(distanceFromPlayer);
                    return;
                }

                if (Time.time >= stateStartTime + lostPlayerIdleDuration)
                    ChangeState(EnemyState.Patrolling);

                return;
            }

            if (!IsPlayerInsideDetection())
            {
                if (currentState == EnemyState.Advancing
                    || currentState == EnemyState.Shooting
                    || currentState == EnemyState.Retreating)
                {
                    isPreparingShot = false;
                    ChangeState(EnemyState.Waiting);
                }

                return;
            }

            ChooseCombatState(distanceFromPlayer);
        }

        private void ChooseCombatState(float distanceFromPlayer)
        {
            if (distanceFromPlayer <= minimumShootingDistance)
            {
                if (currentState == EnemyState.Retreating && hasRetreatDestination)
                    return;

                if (TryCalculateRetreatDestination())
                {
                    isPreparingShot = false;
                    ChangeState(EnemyState.Retreating);
                }
                else
                {
                    if (currentState != EnemyState.Shooting)
                        ChangeState(EnemyState.Shooting);

                    TryShoot();
                }

                return;
            }

            if (distanceFromPlayer > desiredShootingDistance)
            {
                isPreparingShot = false;

                if (currentState != EnemyState.Advancing)
                    ChangeState(EnemyState.Advancing);

                return;
            }

            if (currentState != EnemyState.Shooting)
                ChangeState(EnemyState.Shooting);

            TryShoot();
        }

        private void TryShoot()
        {
            if (isPreparingShot)
                return;

            if (Time.time < lastShotTime + shootCooldown)
                return;

            isPreparingShot = true;
            shotStartTime = Time.time;

            if (animator != null)
                animator.SetTrigger("Shoot");
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
            ChangeState(EnemyState.PostShot);
        }

        private void Shoot()
        {
            if (shootPoint == null || projectilePrefab == null)
                return;

            Vector3 targetPosition = Target.Player.position + Vector3.up * playerChestAimOffset;
            Vector3 shootDirection = targetPosition - shootPoint.position;

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

        private Vector3 GetAdvanceDirection()
        {
            if (!TrySampleNavMeshPosition(Target.Player.position, out Vector3 navMeshPlayerPosition))
                return Vector3.zero;

            return GetNavMeshDirectionTo(navMeshPlayerPosition);
        }

        private bool TryCalculateRetreatDestination()
        {
            Vector3 playerDirection = GetPlayerDirection();
            playerDirection.y = 0f;

            if (playerDirection.sqrMagnitude < 0.01f)
            {
                hasRetreatDestination = false;
                return false;
            }

            playerDirection.Normalize();

            Vector3 desiredRetreatPosition = transform.position - playerDirection * retreatDistance;

            if (!TryGetNearestNavMeshPosition(desiredRetreatPosition, out retreatDestination))
            {
                hasRetreatDestination = false;
                return false;
            }

            Vector3 retreatDirection = GetNavMeshDirectionTo(retreatDestination);

            if (retreatDirection.sqrMagnitude < 0.01f)
            {
                hasRetreatDestination = false;
                return false;
            }

            hasRetreatDestination = true;
            return true;
        }

        private Vector3 GetRetreatDirection()
        {
            if (!hasRetreatDestination)
                return Vector3.zero;

            Vector3 retreatDirection = GetNavMeshDirectionTo(retreatDestination);

            if (retreatDirection.sqrMagnitude < 0.01f)
            {
                hasRetreatDestination = false;
                return Vector3.zero;
            }

            return retreatDirection;
        }

        private bool TryGetNearestNavMeshPosition(Vector3 desiredPosition, out Vector3 navMeshPosition)
        {
            return TrySampleNavMeshPosition(
                desiredPosition,
                retreatNavMeshSampleRadius,
                out navMeshPosition
            );
        }

        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;
            stateStartTime = Time.time;

            if (currentState != EnemyState.Retreating)
                hasRetreatDestination = false;
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
                    MovementDirection = GetNavMeshPatrolDirection();

                    if (IsAtOriginalPosition() && MovementDirection.sqrMagnitude < 0.01f)
                        LookDirection = originalForward;
                    else
                        LookDirection = MovementDirection;

                    break;

                case EnemyState.Advancing:
                    MovementDirection = GetAdvanceDirection();
                    LookDirection = GetPlayerDirection();
                    break;

                case EnemyState.Shooting:
                    LookDirection = GetPlayerDirection();
                    break;

                case EnemyState.PostShot:
                    LookDirection = GetPlayerDirection();
                    break;

                case EnemyState.Retreating:
                    MovementDirection = GetRetreatDirection();
                    LookDirection = GetPlayerDirection();
                    break;

                case EnemyState.Waiting:
                    break;
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null)
                return;

            bool isWalking =
                (currentState == EnemyState.Patrolling || currentState == EnemyState.Advancing)
                && MovementDirection.sqrMagnitude > 0.01f;

            animator.SetBool("IsPatrolling", isWalking);

            animator.SetBool(
                "IsRetreating",
                currentState == EnemyState.Retreating
                && MovementDirection.sqrMagnitude > 0.01f
            );
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (shootPoint == null)
                return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(shootPoint.position, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                shootPoint.position,
                shootPoint.position + shootPoint.forward * 1.5f
            );

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, minimumShootingDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, desiredShootingDistance);
        }
    }
}