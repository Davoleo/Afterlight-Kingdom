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
        private bool hasRetreatDestination;

        private float shotStartTime;
        private float lastShotTime;
        private float stateStartTime;

        private Vector3 retreatDestination;
        private Vector3 originalPosition;
        private Vector3 originalForward;

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead()) return;

            UpdateState();
            UpdateMovementDirection();
            UpdateShot();
            UpdateAnimation();
            MoveAndRotate(Time.deltaTime);
        }

        public override void Activate()
        {
            if (currentState == EnemyState.Sleeping) ChangeState(EnemyState.Patrolling);
        }

        protected override void SetInitialState()
        {
            originalPosition = transform.position;

            originalForward = transform.forward;
            originalForward.y = 0f;

            if (originalForward.sqrMagnitude > 0.01f) originalForward.Normalize();

            ChangeState(startsActive ? EnemyState.Patrolling : EnemyState.Sleeping);
        }

        protected override void OnResetToSpawn()
        {
            isPreparingShot = false;
            hasRetreatDestination = false;

            shotStartTime = 0f;
            lastShotTime = Time.time;

            retreatDestination = Vector3.zero;

            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsRetreating", false);
            animator.ResetTrigger("Shoot");
        }

        protected override float GetCurrentSpeed()
        {
            return currentState switch
            {
                EnemyState.Retreating => retreatSpeed,
                EnemyState.Advancing => advanceSpeed,
                _ => patrolSpeed
            };
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping) return;
            if (currentState == EnemyState.Shooting && isPreparingShot) return;

            float distanceFromPlayer = GetHorizontalDistanceFromPlayer();

            Vector3 horizontalDetectionPosition = transform.position;
            horizontalDetectionPosition.y = Target.Player.position.y;

            bool isPlayerInsideDetection = Target.IsInsideDetection(horizontalDetectionPosition);

            if (currentState == EnemyState.PostShot)
            {
                if (Time.time < stateStartTime + postShotDelay) return;

                if (!isPlayerInsideDetection)
                {
                    ChangeState(EnemyState.Waiting);
                    return;
                }

                ChooseCombatState(distanceFromPlayer);
                return;
            }

            if (currentState == EnemyState.Waiting)
            {
                if (isPlayerInsideDetection)
                {
                    ChooseCombatState(distanceFromPlayer);
                    return;
                }

                if (Time.time >= stateStartTime + lostPlayerIdleDuration) ChangeState(EnemyState.Patrolling);

                return;
            }

            if (!isPlayerInsideDetection)
            {
                if (currentState == EnemyState.Advancing || currentState == EnemyState.Shooting || currentState == EnemyState.Retreating)
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
                if (currentState == EnemyState.Retreating && hasRetreatDestination) return;

                if (TryCalculateRetreatDestination())
                {
                    isPreparingShot = false;
                    ChangeState(EnemyState.Retreating);
                }
                else
                {
                    EnterShootingState();
                }

                return;
            }

            if (distanceFromPlayer > desiredShootingDistance)
            {
                isPreparingShot = false;

                if (currentState != EnemyState.Advancing) ChangeState(EnemyState.Advancing);

                return;
            }

            EnterShootingState();
        }

        private void EnterShootingState()
        {
            if (currentState != EnemyState.Shooting) ChangeState(EnemyState.Shooting);
            TryShoot();
        }

        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            stateStartTime = Time.time;

            if (newState != EnemyState.Retreating) hasRetreatDestination = false;
        }

        private void TryShoot()
        {
            if (isPreparingShot) return;
            if (Time.time < lastShotTime + shootCooldown) return;
            if (!IsCenteredOnGrid()) return;

            isPreparingShot = true;
            shotStartTime = Time.time;

            if (animator != null) animator.SetTrigger("Shoot");
        }

        private void UpdateShot()
        {
            if (!isPreparingShot) return;

            if (currentState != EnemyState.Shooting)
            {
                isPreparingShot = false;
                return;
            }

            if (Time.time < shotStartTime + shootWindup) return;

            isPreparingShot = false;
            lastShotTime = Time.time;

            Shoot();
            ChangeState(EnemyState.PostShot);
        }

        private void Shoot()
        {
            if (shootPoint == null || projectilePrefab == null) return;

            Vector3 targetPosition = Target.Player.position + Vector3.up * playerChestAimOffset;
            Vector3 shootDirection = targetPosition - shootPoint.position;

            if (shootDirection.sqrMagnitude < 0.01f) return;

            shootDirection.Normalize();

            Vector3 spawnPosition = shootPoint.position + shootDirection * projectileSpawnOffset;

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(shootDirection));

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
            return GetNavMeshDirectionTo(Target.Player.position);
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

            if (!TrySampleNavMeshPosition(desiredRetreatPosition, retreatNavMeshSampleRadius, out retreatDestination))
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
            if (!hasRetreatDestination) return Vector3.zero;

            Vector3 direction = GetNavMeshDirectionTo(retreatDestination);

            if (direction.sqrMagnitude >= 0.01f) return direction;

            hasRetreatDestination = false;
            return Vector3.zero;
        }

        private void UpdateMovementDirection()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            switch (currentState)
            {
                case EnemyState.Sleeping:
                case EnemyState.Waiting:
                    break;

                case EnemyState.Patrolling:
                    MovementDirection = GetNavMeshPatrolDirection();
                    LookDirection = IsAtOriginalPosition() && MovementDirection.sqrMagnitude < 0.01f ? originalForward : MovementDirection;
                    break;

                case EnemyState.Advancing:
                    MovementDirection = GetAdvanceDirection();
                    LookDirection = GetPlayerDirection();
                    break;

                case EnemyState.Shooting:
                case EnemyState.PostShot:
                    LookDirection = GetPlayerDirection();
                    break;

                case EnemyState.Retreating:
                    MovementDirection = GetRetreatDirection();
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        private float GetHorizontalDistanceFromPlayer()
        {
            if (!HasPlayer()) return Mathf.Infinity;

            Vector3 difference = Target.Player.position - transform.position;
            difference.y = 0f;

            return difference.magnitude;
        }

        private bool IsAtOriginalPosition()
        {
            Vector3 difference = transform.position - originalPosition;
            difference.y = 0f;

            return difference.sqrMagnitude <= returnPositionTolerance * returnPositionTolerance;
        }

        private void UpdateAnimation()
        {
            bool isMoving = MovementDirection.sqrMagnitude > 0.01f;
            bool isWalking = (currentState == EnemyState.Patrolling || currentState == EnemyState.Advancing) && isMoving;
            bool isRetreating = currentState == EnemyState.Retreating && isMoving;

            animator.SetBool("IsPatrolling", isWalking);
            animator.SetBool("IsRetreating", isRetreating);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (shootPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(shootPoint.position, 0.2f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(shootPoint.position, shootPoint.position + shootPoint.forward * 1.5f);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, minimumShootingDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, desiredShootingDistance);
        }
    }
}