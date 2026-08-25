using UnityEngine;

namespace Enemies
{
    public class ChargingEnemyController : BaseEnemyController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            PreparingCharge,
            Charging,
            Cooldown
        }

        [Header("Charge References")]
        [SerializeField] private Transform chargeHitPoint;

        [Header("Charge Movement")]
        [SerializeField] private float chargeSpeed = 9f;

        [Header("Charge")]
        [SerializeField] private float chargeWindup = 0.45f;
        [SerializeField] private float chargeCooldown = 1.2f;

        [SerializeField] private float chargeHitRadius = 1.1f;
        [SerializeField] private int chargeDamage = 1;
        [SerializeField] private float chargeKnockbackDistance = 5.5f;

        [SerializeField] private float maxChargeHeightDifference = 0.5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float obstacleCheckDistance = 0.35f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private EnemyState currentState;

        private Vector3 chargeDirection;
        private Vector3 chargeDestination;

        private bool hasHitPlayerDuringCharge;
        private float stateStartTime;
        private float chargeEndTime;

        // Used to interrupt the current action only once
        // when the Hit animation starts.
        private bool wasHitAnimationActive;

        // Allows EnemyHealth to use exactly the same Animator
        // used by this controller.
        public Animator EnemyAnimator => animator;

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead())
                return;

            // Hit and Death have priority over every normal enemy action.
            if (HandleDamageAnimationState())
                return;

            UpdateState();
            UpdateMovementDirection();
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
            ChangeState(startsActive ? EnemyState.Patrolling : EnemyState.Sleeping);
        }

        protected override void OnResetToSpawn()
        {
            chargeDirection = Vector3.zero;
            chargeDestination = Vector3.zero;
            hasHitPlayerDuringCharge = false;
            chargeEndTime = 0f;
            wasHitAnimationActive = false;
        }

        protected override float GetCurrentSpeed()
        {
            return currentState == EnemyState.Charging ? chargeSpeed : patrolSpeed;
        }

        private bool IsPlayerAtChargeHeight()
        {
            if (!HasPlayer())
                return false;

            float heightDifference = Mathf.Abs(Target.Player.position.y - transform.position.y);

            return heightDifference <= maxChargeHeightDifference;
        }

        /*
         * Converts the player direction into one
         * of the four cardinal directions:
         * The charger can therefore never charge diagonally.
         */
        private Vector3 GetCardinalPlayerDirection()
        {
            Vector3 direction = Target.Player.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return Vector3.zero;

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
                return new Vector3(Mathf.Sign(direction.x), 0f, 0f);

            return new Vector3(0f, 0f, Mathf.Sign(direction.z));
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping)
                return;

            switch (currentState)
            {
                case EnemyState.Patrolling:
                    if (IsPlayerInsideDetection() && IsPlayerAtChargeHeight())
                        PrepareCharge();

                    break;

                case EnemyState.PreparingCharge:
                    if (IsPlayerOutsideLoseRange() || !IsPlayerAtChargeHeight())
                    {
                        ChangeState(EnemyState.Patrolling);
                        return;
                    }

                    if (Time.time >= stateStartTime + chargeWindup)
                        StartCharge();

                    break;

                case EnemyState.Charging:
                    if (Time.time >= chargeEndTime)
                    {
                        StopCharge();
                        return;
                    }

                    if (HasReachedChargeDestination())
                    {
                        StopCharge();
                        return;
                    }

                    if (IsObstacleInChargeDirection())
                    {
                        StopCharge();
                        return;
                    }

                    CheckChargeHit();
                    break;

                case EnemyState.Cooldown:
                    if (Time.time < stateStartTime + chargeCooldown)
                        return;

                    if (IsPlayerInsideDetection() && IsPlayerAtChargeHeight())
                        PrepareCharge();
                    else
                        ChangeState(EnemyState.Patrolling);

                    break;
            }
        }

        private void PrepareCharge()
        {
            chargeDirection = GetCardinalPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
                return;

            MovementDirection = Vector3.zero;
            LookDirection = chargeDirection;
            hasHitPlayerDuringCharge = false;

            ChangeState(EnemyState.PreparingCharge);
        }

        private void StartCharge()
        {
            if (!IsPlayerAtChargeHeight())
            {
                ChangeState(EnemyState.Patrolling);
                return;
            }

            // Recalculate direction immediately before charging.
            chargeDirection = GetCardinalPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
            {
                ChangeState(EnemyState.Patrolling);
                return;
            }

            if (IsObstacleInChargeDirection())
            {
                StopCharge();
                return;
            }

            /*
             * The maximum charge distance is based on the player's
             * current distance instead of using chargeSpeed as a fixed distance.
             */
            Vector3 playerPosition = Target.Player.position;
            Vector3 currentPosition = transform.position;

            playerPosition.y = 0f;
            currentPosition.y = 0f;

            float distanceToPlayer = Vector3.Distance(currentPosition, playerPosition);

            // Add a small margin so the charge ends slightly beyond the player.
            float maximumChargeDistance = distanceToPlayer + 1f;

            bool destinationFound = false;

            /*
             * Try the longest charge first.
             * If that point is outside the NavMesh (or also with some obstacle),
             * progressively try shorter distances.
             */
            for (float distance = maximumChargeDistance; distance >= 0.5f; distance -= 0.5f)
            {
                Vector3 desiredDestination = transform.position + chargeDirection * distance;

                KeepPositionOnChargeAxis(ref desiredDestination);

                if (TrySampleNavMeshPosition(desiredDestination, out Vector3 sampledDestination))
                {
                    KeepPositionOnChargeAxis(ref sampledDestination);

                    chargeDestination = sampledDestination;
                    destinationFound = true;
                    break;
                }
            }

            if (!destinationFound)
            {
                StopCharge();
                return;
            }

            MovementDirection = GetNavMeshDirectionTo(chargeDestination);

            if (MovementDirection.sqrMagnitude < 0.01f)
            {
                StopCharge();
                return;
            }

            LookDirection = chargeDirection;
            hasHitPlayerDuringCharge = false;

            Vector3 currentPositionForCharge = transform.position;
            Vector3 destinationPosition = chargeDestination;

            currentPositionForCharge.y = 0f;
            destinationPosition.y = 0f;

            float actualChargeDistance = Vector3.Distance(currentPositionForCharge, destinationPosition);
            float actualChargeDuration = actualChargeDistance / chargeSpeed;

            chargeEndTime = Time.time + actualChargeDuration;

            ChangeState(EnemyState.Charging);
        }

        /*
         * Prevents NavMesh sampling from moving the charge destination sideways.
         */
        private void KeepPositionOnChargeAxis(ref Vector3 position)
        {
            if (Mathf.Abs(chargeDirection.x) > 0.01f)
                position.z = transform.position.z;
            else
                position.x = transform.position.x;
        }

        private bool HasReachedChargeDestination()
        {
            if (Mathf.Abs(chargeDirection.x) > 0.01f)
            {
                float distanceX = Mathf.Abs(transform.position.x - chargeDestination.x);
                return distanceX <= 0.15f;
            }

            if (Mathf.Abs(chargeDirection.z) > 0.01f)
            {
                float distanceZ = Mathf.Abs(transform.position.z - chargeDestination.z);
                return distanceZ <= 0.15f;
            }

            return true;
        }

        private void StopCharge()
        {
            MovementDirection = Vector3.zero;
            chargeDirection = Vector3.zero;
            chargeDestination = Vector3.zero;
            chargeEndTime = 0f;

            ChangeState(EnemyState.Cooldown);
        }

        /*
         * Immediately interrupts the current charge or charge preparation
         * when the enemy receives damage.
         */
        private void StopActionForHit()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            chargeDirection = Vector3.zero;
            chargeDestination = Vector3.zero;

            chargeEndTime = 0f;
            hasHitPlayerDuringCharge = false;

            // After the Hit animation the enemy restarts
            // from its normal patrol logic.
            ChangeState(EnemyState.Patrolling);

            // Remove the current NavMesh destination so the enemy
            // does not continue moving during the Hit animation.
            if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }

            // Disable all normal movement/charge animation parameters
            // while Hit has priority.
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsPreparingCharge", false);
            animator.SetBool("IsCharging", false);
        }

        /*
         * Gives Hit and Death priority over the normal state machine.
         * Returns true while the normal enemy logic must remain suspended.
         */
        private bool HandleDamageAnimationState()
        {
            // Death always has maximum priority.
            if (animator.GetBool("IsDead"))
            {
                MovementDirection = Vector3.zero;
                LookDirection = Vector3.zero;

                animator.SetBool("IsPatrolling", false);
                animator.SetBool("IsPreparingCharge", false);
                animator.SetBool("IsCharging", false);

                if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = true;

                return true;
            }

            bool isHit = animator.GetBool("IsHit");

            if (isHit)
            {
                // Interrupt the action only when Hit starts,
                // not on every frame of the animation.
                if (!wasHitAnimationActive)
                {
                    wasHitAnimationActive = true;
                    StopActionForHit();
                }

                return true;
            }

            // Hit animation has just finished.
            if (wasHitAnimationActive)
            {
                wasHitAnimationActive = false;

                if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                    navMeshAgent.isStopped = false;
            }

            return false;
        }

        private bool IsObstacleInChargeDirection()
        {
            if (chargeDirection.sqrMagnitude < 0.01f)
                return false;

            if (obstacleLayer.value == 0)
                return false;

            Vector3 origin = GetBodyCenter();

            return HasBlockingHit(origin, chargeDirection, obstacleCheckDistance, out _);
        }

        private void CheckChargeHit()
        {
            if (hasHitPlayerDuringCharge)
                return;

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + chargeDirection * 0.9f + Vector3.up * 0.7f;

            Collider[] hitColliders = Physics.OverlapSphere(hitPosition, chargeHitRadius, playerLayer);

            foreach (Collider hitCollider in hitColliders)
            {
                if (IsPlayerBlockedByObstacle(hitCollider))
                    continue;

                /*
                 * chargeDirection is cardinal,
                 * so the knockback is cardinal as well.
                 */
                bool damageApplied = TryDamagePlayer(hitCollider, chargeDamage, chargeDirection, true, chargeKnockbackDistance);

                if (!damageApplied)
                    continue;

                hasHitPlayerDuringCharge = true;
                StopCharge();
                return;
            }
        }

        private bool IsPlayerBlockedByObstacle(Collider playerCollider)
        {
            if (obstacleLayer.value == 0)
                return false;

            Vector3 origin = GetBodyCenter();
            Vector3 targetPosition = playerCollider.bounds.center;
            Vector3 direction = targetPosition - origin;
            float distance = direction.magnitude;

            if (distance <= 0.01f)
                return false;

            direction.Normalize();

            return HasBlockingHit(origin, direction, distance, out _);
        }

        private bool HasBlockingHit(Vector3 origin, Vector3 direction, float distance, out Collider blockingCollider)
        {
            blockingCollider = null;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, obstacleLayer, QueryTriggerInteraction.Ignore);

            if (hits.Length == 0)
                return false;

            System.Array.Sort(hits, (firstHit, secondHit) => firstHit.distance.CompareTo(secondHit.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (IsOwnCollider(hit.collider))
                    continue;

                if (Target.IsPlayerCollider(hit.collider))
                    return false;

                blockingCollider = hit.collider;
                return true;
            }

            return false;
        }

        private Vector3 GetBodyCenter()
        {
            return transform.position + Vector3.up * (navMeshAgent.baseOffset + navMeshAgent.height * 0.5f);
        }

        private void ChangeState(EnemyState newState)
        {
            currentState = newState;
            stateStartTime = Time.time;
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
                    if (IsPlayerInsideDetection())
                    {
                        LookDirection = GetPlayerDirection();
                    }
                    else
                    {
                        MovementDirection = GetNavMeshPatrolDirection();
                        LookDirection = MovementDirection;
                    }

                    break;

                case EnemyState.PreparingCharge:
                    LookDirection = chargeDirection.sqrMagnitude > 0.01f ? chargeDirection : GetCardinalPlayerDirection();
                    break;

                case EnemyState.Charging:
                    MovementDirection = GetNavMeshDirectionTo(chargeDestination);
                    LookDirection = chargeDirection;
                    break;

                case EnemyState.Cooldown:
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null)
                return;

            // Hit and Death have priority over the normal state animations.
            if (animator.GetBool("IsHit") || animator.GetBool("IsDead"))
            {
                animator.SetBool("IsPatrolling", false);
                animator.SetBool("IsPreparingCharge", false);
                animator.SetBool("IsCharging", false);
                return;
            }

            animator.SetBool("IsPatrolling", currentState == EnemyState.Patrolling && MovementDirection.sqrMagnitude > 0.01f);
            animator.SetBool("IsPreparingCharge", currentState == EnemyState.PreparingCharge);
            animator.SetBool("IsCharging", currentState == EnemyState.Charging);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + transform.forward * 0.9f + Vector3.up * 0.7f;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitPosition, chargeHitRadius);

            Gizmos.color = Color.red;

            Vector3 debugDirection = Application.isPlaying && chargeDirection.sqrMagnitude > 0.01f ? chargeDirection : transform.forward;

            Vector3 debugCenter = GetBodyCenter();

            Gizmos.DrawLine(debugCenter, debugCenter + debugDirection * obstacleCheckDistance);

            if (Application.isPlaying && currentState == EnemyState.Charging)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(chargeDestination, 0.15f);
                Gizmos.DrawLine(transform.position, chargeDestination);
            }
        }
    }
}