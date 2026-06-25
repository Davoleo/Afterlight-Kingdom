using UnityEngine;

namespace Enemies
{
    public class MeleeEnemyController : EnemyController
    {
        private enum MeleeState
        {
            Sleeping,
            Patrolling,
            Chasing,
            Attacking
        }

        [Header("Melee Movement")]
        [SerializeField] private float chaseSpeed = 4f;

        [Header("Melee Attack")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float attackWindup = 0.5f;

        [Header("Melee Hitbox")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRadius = 0.8f;
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private LayerMask playerLayer;

        private MeleeState currentState;
        private bool isPreparingAttack;
        private float attackStartTime;
        private float lastAttackTime;

        public override void Activate()
        {
            if (currentState == MeleeState.Sleeping)
                ChangeState(MeleeState.Patrolling);
        }

        protected override void SetInitialState()
        {
            ChangeState(startsActive ? MeleeState.Patrolling : MeleeState.Sleeping);
        }

        protected override void UpdateEnemy()
        {
            UpdateState();
            UpdateAttack();
        }

        protected override void ResetSpecificState()
        {
            isPreparingAttack = false;
            lastAttackTime = Time.time;
        }

        protected override void SetStateAfterReset()
        {
            ChangeState(startsActive ? MeleeState.Patrolling : MeleeState.Sleeping);
        }

        private void UpdateState()
        {
            if (currentState == MeleeState.Sleeping)
                return;

            if (IsPlayerDead())
                return;

            float distanceFromPlayer = DistanceFromPlayer();

            if (distanceFromPlayer <= attackRange)
            {
                ChangeState(MeleeState.Attacking);
                TryAttack();
                return;
            }

            if (currentState == MeleeState.Attacking && distanceFromPlayer > attackRange)
            {
                isPreparingAttack = false;
                ChangeState(MeleeState.Chasing);
                return;
            }

            if (distanceFromPlayer <= target.DetectionRange)
            {
                ChangeState(MeleeState.Chasing);
                return;
            }

            if (currentState == MeleeState.Chasing && distanceFromPlayer >= target.LosePlayerRange)
                ChangeState(MeleeState.Patrolling);
        }

        private void TryAttack()
        {
            if (isPreparingAttack)
                return;

            if (Time.time < lastAttackTime + attackCooldown)
                return;

            isPreparingAttack = true;
            attackStartTime = Time.time;
        }

        private void UpdateAttack()
        {
            if (!isPreparingAttack)
                return;

            if (currentState != MeleeState.Attacking)
            {
                isPreparingAttack = false;
                return;
            }

            if (Time.time < attackStartTime + attackWindup)
                return;

            isPreparingAttack = false;
            lastAttackTime = Time.time;

            PerformAttack();
        }

        private void PerformAttack()
        {
            if (attackPoint == null)
                return;

            Vector3 knockbackDirection = GetPlayerDirection();

            EnemyPlayerDamage.TryDamageFirstInSphere(
                attackPoint.position,
                attackRadius,
                playerLayer,
                attackDamage,
                knockbackDirection,
                true
            );
        }

        private void ChangeState(MeleeState newState)
        {
            currentState = newState;
        }

        protected override void UpdateMovementDirection()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            switch (currentState)
            {
                case MeleeState.Sleeping:
                    break;

                case MeleeState.Patrolling:
                    MovementDirection = GetPatrolDirection();
                    LookDirection = MovementDirection;
                    break;

                case MeleeState.Chasing:
                    MovementDirection = GetPlayerDirection();
                    LookDirection = MovementDirection;
                    break;

                case MeleeState.Attacking:
                    MovementDirection = Vector3.zero;
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        protected override float GetCurrentSpeed()
        {
            return currentState == MeleeState.Chasing ? chaseSpeed : patrolSpeed;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (attackPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            }
        }
    }
}