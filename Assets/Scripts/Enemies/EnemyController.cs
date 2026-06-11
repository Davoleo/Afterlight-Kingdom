using KinematicCharacterController;
using Player;
using UnityEngine;

namespace Enemies
{
    public class EnemyController : MonoBehaviour, ICharacterController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            Chasing,
            Attacking
        }

        [Header("References")]
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform player;
        [SerializeField] private Transform leftPatrolPoint;
        [SerializeField] private Transform rightPatrolPoint;

        [Header("Activation")]
        [SerializeField] private bool startsActive = true;
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float losePlayerRange = 8f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float patrolPointTolerance = 0.25f;

        [Header("Melee")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float attackWindup = 0.5f;

        [Header("Melee Hitbox")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRadius = 0.8f;
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private LayerMask playerLayer;

        [Header("Gravity")]
        [SerializeField] private float gravity = 20f;

        private EnemyState currentState;
        private Transform currentPatrolTarget;

        private Vector3 spawnPosition;
        private Vector3 movementDirection;
        private Vector3 lookDirection;

        private bool isPreparingAttack;
        private float attackStartTime;
        private float lastAttackTime;

        private void Awake()
        {
            if (motor == null)
                motor = GetComponent<KinematicCharacterMotor>();

            motor.CharacterController = this;
        }

        private void Start()
        {
            spawnPosition = transform.position;
            currentState = startsActive ? EnemyState.Patrolling : EnemyState.Sleeping;
            currentPatrolTarget = rightPatrolPoint;

            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                    player = playerObject.transform;
            }
        }

        private void Update()
        {
            CheckPlayerDeathOrFall();

            UpdateState();
            UpdateMovementDirection();
            UpdateAttack();
        }

        public void Activate()
        {
            if (currentState == EnemyState.Sleeping)
                currentState = EnemyState.Patrolling;
        }

        private void CheckPlayerDeathOrFall()
        {
            if (player == null)
                return;

            HealthManager playerHealth = player.GetComponentInParent<HealthManager>();

            if (playerHealth != null && playerHealth.Health <= 0)
                ResetToSpawn();
        }

        private void ResetToSpawn()
        {
            isPreparingAttack = false;

            if (motor != null)
                motor.SetPosition(spawnPosition);
            else
                transform.position = spawnPosition;

            currentState = startsActive ? EnemyState.Patrolling : EnemyState.Sleeping;
            currentPatrolTarget = rightPatrolPoint;

            movementDirection = Vector3.zero;
            lookDirection = Vector3.zero;

            lastAttackTime = Time.time;

            Debug.Log("Enemy reset to spawn.");
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping)
                return;

            if (player == null)
                return;

            HealthManager playerHealth = player.GetComponentInParent<HealthManager>();

            if (playerHealth != null && playerHealth.Health <= 0)
                return;

            float distanceFromPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceFromPlayer <= attackRange)
            {
                currentState = EnemyState.Attacking;
                TryAttack();
                return;
            }

            if (currentState == EnemyState.Attacking && distanceFromPlayer > attackRange)
            {
                currentState = EnemyState.Chasing;
                isPreparingAttack = false;
                return;
            }

            if (distanceFromPlayer <= detectionRange)
            {
                currentState = EnemyState.Chasing;
                return;
            }

            if (currentState == EnemyState.Chasing && distanceFromPlayer >= losePlayerRange)
            {
                currentState = EnemyState.Patrolling;
            }
        }

        private void TryAttack()
        {
            if (isPreparingAttack)
                return;

            if (Time.time < lastAttackTime + attackCooldown)
                return;

            isPreparingAttack = true;
            attackStartTime = Time.time;

            Debug.Log("Preparing attack...");
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
            Debug.Log("ATTACK!");

            if (attackPoint == null)
            {
                Debug.LogWarning("AttackPoint non assegnato nell'EnemyController.");
                return;
            }

            Collider[] hitColliders = Physics.OverlapSphere(
                attackPoint.position,
                attackRadius,
                playerLayer
            );

            if (hitColliders.Length == 0)
            {
                Debug.Log("Attack eseguito, ma nessun collider Player trovato nella hitbox.");
                return;
            }

            foreach (Collider hitCollider in hitColliders)
            {
                PlayerDamageFeedback damageFeedback =
                    hitCollider.GetComponentInParent<PlayerDamageFeedback>();

                if (damageFeedback != null)
                {
                    Vector3 knockbackDirection =
                        hitCollider.transform.position - transform.position;

                    bool damageApplied =
                        damageFeedback.TryTakeDamage(attackDamage, knockbackDirection, true);

                    if (damageApplied)
                        Debug.Log("Player hit by melee enemy!");
                    else
                        Debug.Log("Melee hit ignored: player invincible.");

                    continue;
                }

                HealthManager healthManager =
                    hitCollider.GetComponentInParent<HealthManager>();

                if (healthManager == null)
                {
                    Debug.Log("Collider trovato, ma HealthManager non trovato nel parent.");
                    continue;
                }

                healthManager.TakeDamage(attackDamage);
                Debug.LogWarning("PlayerDamageFeedback non trovato, usato HealthManager diretto.");
            }
        }

        private void UpdateMovementDirection()
        {
            movementDirection = Vector3.zero;
            lookDirection = Vector3.zero;

            switch (currentState)
            {
                case EnemyState.Sleeping:
                    break;

                case EnemyState.Patrolling:
                    movementDirection = GetPatrolDirection();
                    lookDirection = movementDirection;
                    break;

                case EnemyState.Chasing:
                    movementDirection = GetChaseDirection();
                    lookDirection = movementDirection;
                    break;

                case EnemyState.Attacking:
                    movementDirection = Vector3.zero;
                    lookDirection = GetChaseDirection();
                    break;
            }
        }

        private Vector3 GetPatrolDirection()
        {
            if (leftPatrolPoint == null || rightPatrolPoint == null)
                return Vector3.zero;

            Vector3 targetPosition = currentPatrolTarget.position;
            Vector3 directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.magnitude <= patrolPointTolerance)
                SwitchPatrolTarget();

            return directionToTarget.normalized;
        }

        private void SwitchPatrolTarget()
        {
            currentPatrolTarget = currentPatrolTarget == rightPatrolPoint
                ? leftPatrolPoint
                : rightPatrolPoint;
        }

        private Vector3 GetChaseDirection()
        {
            if (player == null)
                return Vector3.zero;

            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0f;

            return directionToPlayer.normalized;
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            float targetSpeed = currentState == EnemyState.Chasing ? chaseSpeed : patrolSpeed;
            Vector3 targetVelocity = movementDirection * targetSpeed;

            if (motor.GroundingStatus.IsStableOnGround)
            {
                currentVelocity = Vector3.Lerp(
                    currentVelocity,
                    targetVelocity,
                    1f - Mathf.Exp(-acceleration * deltaTime)
                );
            }
            else
            {
                currentVelocity.x = targetVelocity.x;
                currentVelocity.z = targetVelocity.z;
                currentVelocity.y -= gravity * deltaTime;
            }
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (lookDirection.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            currentRotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                1f - Mathf.Exp(-acceleration * deltaTime)
            );
        }

        public void BeforeCharacterUpdate(float deltaTime) { }
        public void PostGroundingUpdate(float deltaTime) { }
        public void AfterCharacterUpdate(float deltaTime) { }

        public bool IsColliderValidForCollisions(Collider coll) => true;

        public void OnGroundHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        { }

        public void OnMovementHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        { }

        public void ProcessHitStabilityReport(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation,
            ref HitStabilityReport hitStabilityReport)
        { }

        public void OnDiscreteCollisionDetected(Collider hitCollider) { }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, losePlayerRange);

            if (leftPatrolPoint != null && rightPatrolPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(leftPatrolPoint.position, rightPatrolPoint.position);
            }

            if (attackPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            }
        }
    }
}