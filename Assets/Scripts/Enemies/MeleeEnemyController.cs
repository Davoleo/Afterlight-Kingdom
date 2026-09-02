using UnityEngine;

namespace Enemies
{
    public class MeleeEnemyController : BaseEnemyController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            Chasing,
            Attacking,
            Hit
        }

        [Header("Melee Movement")]
        [SerializeField] private float chaseSpeed = 4f;

        [Header("Melee Attack")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float attackWindup = 0.35f;

        // Time during which the enemy remains completely still
        // after performing the attack.
        [SerializeField] private float attackRecoveryDuration = 1f;

        [Header("Melee Hitbox")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRadius = 0.8f;
        [SerializeField] private int attackDamage = 1;

        private const float HitStopDuration = 1f;

        private EnemyState currentState;
        private EnemyState stateBeforeHit;

        private bool isPreparingAttack;
        private float attackStartTime;
        private float lastAttackTime;
        private bool isRecoveringAttack;
        private float attackRecoveryEndTime;
        private Vector3 attackPosition;
        private bool hasLockedAttackPosition;

        private bool wasHit;
        private float hitStopEndTime;
        private Vector3 hitPosition;
        private bool hasLockedHitPosition;

        protected override void Start()
        {
            base.Start();

            animator.applyRootMotion = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead())
                return;

            if (HandleHitState())
                return;

            UpdateState();
            UpdateMovementDirection();
            UpdateAnimation();
            UpdateAttack();

            /*
             * During the attack and the recovery time
             * the enemy remains completely still.
             */
            if (currentState == EnemyState.Attacking)
            {
                KeepAttackPosition();
                return;
            }

            MoveAndRotate(Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (currentState == EnemyState.Hit)
            {
                KeepHitPosition();
                return;
            }

            if (currentState != EnemyState.Attacking)
                return;

            if (!hasLockedAttackPosition)
                return;

            // Keep the enemy locked even after the Animator update.
            KeepAttackPosition();
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
            isPreparingAttack = false;
            isRecoveringAttack = false;

            attackStartTime = 0f;
            lastAttackTime = Time.time;
            attackRecoveryEndTime = 0f;

            hasLockedAttackPosition = false;
            attackPosition = Vector3.zero;

            wasHit = false;
            hitStopEndTime = 0f;
            hasLockedHitPosition = false;
            hitPosition = Vector3.zero;
            stateBeforeHit = EnemyState.Sleeping;

            // Remove any attack still pending when the player dies.
            animator.ResetTrigger("Attack");
            animator.SetBool("IsMoving", false);
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
        }

        protected override float GetCurrentSpeed()
        {
            return currentState == EnemyState.Chasing ? chaseSpeed : patrolSpeed;
        }

        /*
         * Hit completely overrides the normal melee state machine.
         * The enemy remains locked for at least one second before
         * being allowed to enter another state.
         */
        private bool HandleHitState()
        {
            bool isHit = animator.GetBool("IsHit");

            // Detect the moment the enemy receives damage.
            if (isHit && !wasHit)
            {
                wasHit = true;

                if (currentState != EnemyState.Hit)
                    stateBeforeHit = currentState;

                hitStopEndTime = Time.time + HitStopDuration;
                hitPosition = transform.position;
                hasLockedHitPosition = true;

                isPreparingAttack = false;
                isRecoveringAttack = false;
                hasLockedAttackPosition = false;

                MovementDirection = Vector3.zero;
                LookDirection = Vector3.zero;

                animator.ResetTrigger("Attack");
                animator.SetBool("IsMoving", false);

                ChangeState(EnemyState.Hit);
            }

            if (!isHit)
                wasHit = false;

            if (currentState != EnemyState.Hit)
                return false;

            KeepHitPosition();

            /*
             * The enemy cannot leave Hit before one full second
             * has elapsed and the Hit animation has finished.
             */
            if (Time.time < hitStopEndTime || isHit)
                return true;

            hasLockedHitPosition = false;

            if (stateBeforeHit == EnemyState.Sleeping)
            {
                ChangeState(EnemyState.Sleeping);
            }
            else if (IsPlayerInsideDetection())
            {
                ChangeState(EnemyState.Chasing);
            }
            else
            {
                ChangeState(EnemyState.Patrolling);
            }

            return false;
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping)
                return;

            /*
             * While preparing the attack the enemy cannot
             * leave the Attacking state.
             */
            if (currentState == EnemyState.Attacking && isPreparingAttack)
                return;

            /*
             * After performing the attack, keep the enemy
             * completely still for attackRecoveryDuration.
             */
            if (currentState == EnemyState.Attacking && isRecoveringAttack)
            {
                if (Time.time < attackRecoveryEndTime)
                    return;

                isRecoveringAttack = false;
            }

            float distanceFromPlayer = GetDistanceFromPlayer();

            if (distanceFromPlayer <= attackRange)
            {
                if (currentState != EnemyState.Attacking)
                    ChangeState(EnemyState.Attacking);

                TryAttack();
                return;
            }

            if (currentState == EnemyState.Attacking)
            {
                ChangeState(EnemyState.Chasing);

                isPreparingAttack = false;
                isRecoveringAttack = false;
            }

            if (IsPlayerInsideDetection())
            {
                ChangeState(EnemyState.Chasing);
                return;
            }

            if (currentState == EnemyState.Chasing && IsPlayerOutsideLoseRange())
                ChangeState(EnemyState.Patrolling);
        }

        private void TryAttack()
        {
            if (isPreparingAttack)
                return;

            if (isRecoveringAttack)
                return;

            if (Time.time < lastAttackTime + attackCooldown)
                return;

            /*
             * Save the player's direction exactly when the attack starts.
             * The enemy does not rotate to follow the player afterwards.
             */
            Vector3 attackDirection = GetPlayerDirection();
            attackDirection.y = 0f;

            if (attackDirection.sqrMagnitude < 0.01f)
                attackDirection = transform.forward;
            else
                attackDirection.Normalize();

            // Lock the exact position where the attack starts.
            attackPosition = transform.position;
            hasLockedAttackPosition = true;

            StopMovementImmediately();

            if (attackDirection.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(attackDirection, Vector3.up);

            isPreparingAttack = true;
            attackStartTime = Time.time;

            if (animator != null)
                animator.SetTrigger("Attack");
        }

        private void UpdateAttack()
        {
            if (!isPreparingAttack)
                return;

            if (currentState != EnemyState.Attacking)
            {
                isPreparingAttack = false;
                return;
            }

            if (Time.time < attackStartTime + attackWindup)
                return;

            isPreparingAttack = false;
            lastAttackTime = Time.time;

            PerformAttack();

            /*
             * After performing the attack, keep the enemy
             * completely still for the recovery duration.
             */
            isRecoveringAttack = true;
            attackRecoveryEndTime = Time.time + attackRecoveryDuration;
        }

        private void PerformAttack()
        {
            Vector3 hitPosition = attackPoint != null ? attackPoint.position : transform.position + transform.forward * attackRange * 0.5f + Vector3.up * navMeshAgent.height * 0.5f;

            Collider[] hitColliders = Physics.OverlapSphere(hitPosition, attackRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);

            foreach (Collider hitCollider in hitColliders)
            {
                if (!Target.IsPlayerCollider(hitCollider))
                    continue;

                Vector3 knockbackDirection = hitCollider.bounds.center - transform.position;
                knockbackDirection.y = 0f;

                if (knockbackDirection.sqrMagnitude < 0.01f)
                    knockbackDirection = transform.forward;

                bool damageApplied = TryDamagePlayer(hitCollider, attackDamage, knockbackDirection.normalized, true);

                if (damageApplied)
                    return;
            }
        }

        private void StopMovementImmediately()
        {
            MovementDirection = Vector3.zero;

            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
        }

        /*
         * Keeps the enemy exactly at the position
         * where the hit started.
         */
        private void KeepHitPosition()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            if (!hasLockedHitPosition)
                return;

            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                transform.position = hitPosition;
                return;
            }

            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;

            if ((transform.position - hitPosition).sqrMagnitude > 0.000001f)
                navMeshAgent.Warp(hitPosition);
        }

        /*
         * Keeps the enemy exactly at the position
         * where the attack started.
         */
        private void KeepAttackPosition()
        {
            MovementDirection = Vector3.zero;

            if (!hasLockedAttackPosition)
                return;

            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                transform.position = attackPosition;
                return;
            }

            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;

            if ((transform.position - attackPosition).sqrMagnitude > 0.000001f)
                navMeshAgent.Warp(attackPosition);
        }

        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState)
                return;

            if (newState == EnemyState.Attacking)
            {
                attackPosition = transform.position;
                hasLockedAttackPosition = true;

                StopMovementImmediately();
            }

            if (currentState == EnemyState.Attacking && newState != EnemyState.Attacking)
            {
                hasLockedAttackPosition = false;
                isRecoveringAttack = false;

                if (newState != EnemyState.Hit && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.velocity = Vector3.zero;
                    navMeshAgent.isStopped = false;
                }
            }

            if (newState == EnemyState.Hit)
                StopMovementImmediately();

            if (currentState == EnemyState.Hit && newState != EnemyState.Hit)
            {
                if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.velocity = Vector3.zero;
                    navMeshAgent.isStopped = false;
                }
            }

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
                    MovementDirection = GetNavMeshPatrolDirection();
                    LookDirection = MovementDirection;
                    break;

                case EnemyState.Chasing:
                    MovementDirection = GetNavMeshDirectionTo(Target.Player.position);
                    LookDirection = MovementDirection;
                    break;

                case EnemyState.Attacking:
                    // No movement or continuous rotation during attack.
                    MovementDirection = Vector3.zero;
                    LookDirection = Vector3.zero;
                    break;

                case EnemyState.Hit:
                    // No movement while the Hit state is active.
                    MovementDirection = Vector3.zero;
                    LookDirection = Vector3.zero;
                    break;
            }
        }

        private void UpdateAnimation()
        {
            bool isMoving = currentState != EnemyState.Attacking && MovementDirection.sqrMagnitude > 0.01f;
            animator.SetBool("IsMoving", isMoving);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}