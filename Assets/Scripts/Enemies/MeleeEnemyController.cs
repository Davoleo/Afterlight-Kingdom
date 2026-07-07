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

        private EnemyState currentState;
        private bool isPreparingAttack;
        private float attackStartTime;
        private float lastAttackTime;

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead())
                return;

            UpdateState();
            UpdateMovementDirection();
            UpdateAttack();

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
            isPreparingAttack = false;
            lastAttackTime = Time.time;
        }

        protected override float GetCurrentSpeed()
        {
            return currentState == EnemyState.Chasing ? chaseSpeed : patrolSpeed;
        }

        private void UpdateState()
        {
            float distanceFromPlayer = GetDistanceFromPlayer();

            if (currentState == EnemyState.Sleeping)
                return;

            if (distanceFromPlayer <= attackRange)
            {
                if (currentState != EnemyState.Attacking)
                    ChangeState(EnemyState.Attacking);

                TryAttack();
                return;
            }

            if (currentState == EnemyState.Attacking && distanceFromPlayer > attackRange)
            {
                ChangeState(EnemyState.Chasing);
                isPreparingAttack = false;
                return;
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

            if (Time.time < lastAttackTime + attackCooldown)
                return;

            isPreparingAttack = true;
            attackStartTime = Time.time;
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
        }

        private void PerformAttack()
        {
            Collider[] hitColliders = Physics.OverlapSphere(
                attackPoint.position,
                attackRadius,
                playerLayer
            );

            if (hitColliders.Length == 0)
            {
                return;
            }

            foreach (Collider hitCollider in hitColliders)
            {
                Vector3 knockbackDirection = hitCollider.bounds.center - transform.position;
                knockbackDirection.y = 0f;

                if (knockbackDirection.sqrMagnitude < 0.01f)
                    knockbackDirection = transform.forward;

                bool damageApplied = TryDamagePlayer(
                    hitCollider,
                    attackDamage,
                    knockbackDirection.normalized,
                    true
                );

                if (!damageApplied)
                    continue;

                return;
            }
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

                case EnemyState.Chasing:
                    MovementDirection = GetPlayerDirection();
                    LookDirection = MovementDirection;
                    break;

                case EnemyState.Attacking:
                    MovementDirection = Vector3.zero;
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (attackPoint == null)
                return;

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}