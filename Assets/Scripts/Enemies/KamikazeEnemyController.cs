using UnityEngine;

namespace Enemies
{
    public class KamikazeEnemyController : BaseEnemyController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            PreparingCharge,
            Charging
        }

        [Header("Charge References")]
        [SerializeField] private Transform chargeHitPoint;
        [SerializeField] private GameObject explosionPrefab;

        [Header("Charge Movement")]
        [SerializeField] private float chargeSpeed = 9f;

        [Header("Charge")]
        [SerializeField] private float chargeWindup = 1.5f;

        [SerializeField] private float chargeHitRadius = 1.1f;
        [SerializeField] private int chargeDamage = 2;

        [SerializeField] private float maxChargeHeightDifference = 0.5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float obstacleCheckWidth = 1f;
        [SerializeField] private float obstacleCheckHeight = 2f;
        [SerializeField] private float obstacleCheckDepth = 1f;
        [SerializeField] private float obstacleCheckForwardOffset = 0.2f;

        [Header("Charge Stuck Detection")]
        [SerializeField] private float stuckMovementThreshold = 0.02f;
        [SerializeField] private float stuckDuration = 0.25f;

        private EnemyState currentState;

        private Vector3 chargeDirection;

        private float stateStartTime;

        private EnemyHealth enemyHealth;

        private Vector3 lastChargePosition;
        private float stuckTime;

        private bool isBackstepping;
        private Vector3 chargeBackstepTarget;

        // Used to interrupt the current action only once
        // when the Hit animation starts.
        private bool wasHitAnimationActive;

        // Allows EnemyHealth to use exactly the same Animator
        // used by this controller.

        protected override void Start()
        {
            base.Start();

            enemyHealth = GetComponent<EnemyHealth>();

            enemyHealth.Died += HandleDeath;
        }

        private void OnDestroy()
        {
            enemyHealth.Died -= HandleDeath;
        }

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead()) return;

            // Hit and Death have priority over every normal enemy action.
            if (HandleDamageAnimationState()) return;

            UpdateState();
            UpdateMovementDirection();
            UpdateAnimation();

            // During the charge the enemy moves directly along the fixed charge direction,
            // without using grid pathfinding, so the trajectory can never curve.
            if (currentState == EnemyState.Charging) MoveChargeStraight(Time.deltaTime);
            else MoveAndRotate(Time.deltaTime);
        }

        public override void Activate()
        {
            if (currentState == EnemyState.Sleeping) ChangeState(EnemyState.Patrolling);
        }

        protected override void SetInitialState()
        {
            ChangeState(startsActive ? EnemyState.Patrolling : EnemyState.Sleeping);
        }

        protected override void OnResetToSpawn()
        {
            chargeDirection = Vector3.zero;
            isBackstepping = false;
            wasHitAnimationActive = false;
        }

        private bool IsPlayerAtChargeHeight()
        {
            if (!HasPlayer()) return false;

            float heightDifference = Mathf.Abs(Target.Player.position.y - transform.position.y);

            return heightDifference <= maxChargeHeightDifference;
        }

        /*
         * Converts the player direction into one
         * of the four cardinal directions:
         * The kamikaze can therefore never charge diagonally.
         */
        private Vector3 GetCardinalPlayerDirection()
        {
            Vector3 direction = Target.Player.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f) return Vector3.zero;

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.z)) return new Vector3(Mathf.Sign(direction.x), 0f, 0f);

            return new Vector3(0f, 0f, Mathf.Sign(direction.z));
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping) return;

            switch (currentState)
            {
                case EnemyState.Patrolling:
                    if (IsPlayerInsideDetection() && IsPlayerAtChargeHeight()) PrepareCharge();

                    break;

                case EnemyState.PreparingCharge:
                    if (IsPlayerOutsideLoseRange() || !IsPlayerAtChargeHeight())
                    {
                        isBackstepping = false;
                        ChangeState(EnemyState.Patrolling);
                        return;
                    }
                    UpdateChargeBackstep();

                    if (Time.time >= stateStartTime + chargeWindup) StartCharge();

                    break;

                case EnemyState.Charging:
                    if (IsObstacleInChargeDirection() || IsChargeStuck())
                    {
                        KillSelf();
                        return;
                    }

                    CheckChargeHit();
                    break;
            }
        }

        private void PrepareCharge()
        {
            chargeDirection = GetCardinalPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f) return;

            MovementDirection = Vector3.zero;
            LookDirection = chargeDirection;
            isBackstepping = true;
            chargeBackstepTarget = transform.position - chargeDirection;

            ChangeState(EnemyState.PreparingCharge);
        }
        private void UpdateChargeBackstep()
        {
            if (!isBackstepping) return;

            float backstepSpeed = 2f / chargeWindup;
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, chargeBackstepTarget, backstepSpeed * Time.deltaTime);

            navMeshAgent.Move(nextPosition - transform.position);

            if ((transform.position - chargeBackstepTarget).sqrMagnitude <= 0.0001f)
                isBackstepping = false;
        }

        private void StartCharge()
        {
            if (!IsPlayerAtChargeHeight())
            {
                isBackstepping = false;
                ChangeState(EnemyState.Patrolling);
                return;
            }

            // Recalculate direction immediately before charging.
            chargeDirection = GetCardinalPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
            {
                isBackstepping = false;
                ChangeState(EnemyState.Patrolling);
                return;
            }

            if (IsObstacleInChargeDirection())
            {
                KillSelf();
                return;
            }
            isBackstepping = false;

            // During the charge the movement direction is always the original cardinal direction and is never recalculated by the pathfinding.
            MovementDirection = chargeDirection;

            LookDirection = chargeDirection;

            lastChargePosition = transform.position;
            stuckTime = 0f;

            ChangeState(EnemyState.Charging);
        }

        // Moves the kamikaze directly along the fixed cardinal charge direction. This bypasses grid pathfinding so the charge cannot turn or adapt its path.
        private void MoveChargeStraight(float deltaTime)
        {
            navMeshAgent.Move(chargeDirection * chargeSpeed * deltaTime);
        }

        private bool IsChargeStuck()
        {
            float movedDistance = Vector3.Distance(transform.position, lastChargePosition);

            if (movedDistance <= stuckMovementThreshold)
                stuckTime += Time.deltaTime;
            else
            {
                stuckTime = 0f;
                lastChargePosition = transform.position;
            }

            return stuckTime >= stuckDuration;
        }

        //Immediately interrupts the current charge or charge preparation when the enemy receives damage.

        private void StopActionForHit()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            chargeDirection = Vector3.zero;
            stuckTime = 0f;
            isBackstepping = false;

            // After the Hit animation the enemy restarts from its normal patrol logic.
            ChangeState(EnemyState.Patrolling);

            // Remove the current NavMesh destination so the enemy does not continue moving during the Hit animation.

            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();


            // Disable all normal movement/charge animation parameters while Hit has priority.
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsPreparingCharge", false);
            animator.SetBool("IsCharging", false);
        }

        //Gives Death priority over the normal state machine. Returns true while the normal enemy logic must remain suspended.

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

                navMeshAgent.isStopped = true;

                return true;
            }

            bool isHit = animator.GetBool("IsHit");

            if (isHit)
            {
                // Interrupt the action only when Hit starts, not on every frame of the animation.
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

            Vector3 halfExtents = new Vector3(obstacleCheckWidth * 0.5f, obstacleCheckHeight * 0.5f, obstacleCheckDepth * 0.5f);

            //Places the obstacle detection box exactly one block in front of the kamikaze
            Vector3 boxCenter = transform.position + chargeDirection * (1f + obstacleCheckForwardOffset) + Vector3.up * ((obstacleCheckHeight * 0.5f) - navMeshAgent.baseOffset);

            Collider[] obstacles = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity, obstacleLayer, QueryTriggerInteraction.Ignore);

            foreach (Collider obstacle in obstacles)
            {
                if (IsOwnCollider(obstacle)) continue;
                if (Target.IsPlayerCollider(obstacle)) continue;

                return true;
            }

            return false;
        }

        private void CheckChargeHit()
        {
            Vector3 hitPosition = chargeHitPoint != null ? chargeHitPoint.position : transform.position + chargeDirection * 0.9f + Vector3.up * 0.7f;

            Collider[] hitColliders = Physics.OverlapSphere(hitPosition, chargeHitRadius, playerLayer);

            foreach (Collider hitCollider in hitColliders)
            {
                if (IsPlayerBlockedByObstacle(hitCollider))
                    continue;

                TryDamagePlayer(hitCollider, chargeDamage, Vector3.zero, false, 0f);

                KillSelf();
                return;
            }
        }

        private void KillSelf()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;
            chargeDirection = Vector3.zero;

            navMeshAgent.isStopped = true;
            enemyHealth.TakeDamage(enemyHealth.CurrentHealth);
        }

        private void HandleDeath()
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
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

            return HasBlockingHit(origin, direction, distance);
        }

        private bool HasBlockingHit(Vector3 origin, Vector3 direction, float distance)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, obstacleLayer, QueryTriggerInteraction.Ignore);

            if (hits.Length == 0)
                return false;

            System.Array.Sort(hits, (firstHit, secondHit) => firstHit.distance.CompareTo(secondHit.distance));

            foreach (RaycastHit hit in hits)
            {
                if (IsOwnCollider(hit.collider))
                    continue;
                if (Target.IsPlayerCollider(hit.collider))
                    return false;

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
                        LookDirection = GetPlayerDirection();
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
                    // Keep the original cardinal charge direction fixed. Do not ask the NavMesh pathfinding for a new direction.
                    MovementDirection = chargeDirection;
                    LookDirection = chargeDirection;
                    break;
            }
        }

        private void UpdateAnimation()
        {
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

            Gizmos.color = Color.red;

            Vector3 debugDirection = Application.isPlaying && chargeDirection.sqrMagnitude > 0.01f ? chargeDirection : transform.forward;

            debugDirection.y = 0f;

            if (debugDirection.sqrMagnitude > 0.01f)
            {
                debugDirection.Normalize();

                float agentBaseOffset = navMeshAgent != null ? navMeshAgent.baseOffset : 0f;

                Vector3 boxCenter = transform.position + debugDirection * (1f + obstacleCheckForwardOffset) + Vector3.up * ((obstacleCheckHeight * 0.5f) - agentBaseOffset);

                Vector3 boxSize = new Vector3(obstacleCheckWidth, obstacleCheckHeight, obstacleCheckDepth);

                Gizmos.DrawWireCube(boxCenter, boxSize);
            }
        }
    }
}