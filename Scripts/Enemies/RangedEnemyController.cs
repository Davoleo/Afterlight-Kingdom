using KinematicCharacterController;
using Player;
using UnityEngine;

namespace Enemies
{
    public class RangedEnemyController : MonoBehaviour, ICharacterController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            Shooting
        }

        [Header("References")]
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform player;
        [SerializeField] private Transform leftPatrolPoint;
        [SerializeField] private Transform rightPatrolPoint;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject projectilePrefab;

        [Header("Activation")]
        [SerializeField] private bool startsActive = true;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float losePlayerRange = 13f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float patrolPointTolerance = 0.25f;

        [Header("Shooting")]
        [SerializeField] private float shootCooldown = 1.2f;
        [SerializeField] private float shootWindup = 0.25f;

        [Header("Gravity")]
        [SerializeField] private float gravity = 20f;

        private EnemyState currentState;

        private Vector3 spawnPosition;
        private Vector3 movementDirection;
        private Vector3 lookDirection;

        private Vector3 leftPatrolPosition;
        private Vector3 rightPatrolPosition;
        private Vector3 currentPatrolPosition;

        private bool hasValidPatrolPoints;

        private bool isPreparingShot;
        private float shotStartTime;
        private float lastShotTime;

        private void Awake()
        {
            if (motor == null)
                motor = GetComponent<KinematicCharacterMotor>();

            motor.CharacterController = this;
        }

        private void Start()
        {
            spawnPosition = transform.position;
            CachePatrolPositions();

            currentState = startsActive ? EnemyState.Patrolling : EnemyState.Sleeping;

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
            UpdateShot();
        }

        private void CachePatrolPositions()
        {
            hasValidPatrolPoints = leftPatrolPoint != null && rightPatrolPoint != null;

            if (!hasValidPatrolPoints)
            {
                leftPatrolPosition = transform.position;
                rightPatrolPosition = transform.position;
                currentPatrolPosition = transform.position;
                return;
            }

            leftPatrolPosition = leftPatrolPoint.position;
            rightPatrolPosition = rightPatrolPoint.position;
            currentPatrolPosition = rightPatrolPosition;
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
            isPreparingShot = false;

            if (motor != null)
                motor.SetPosition(spawnPosition);
            else
                transform.position = spawnPosition;

            currentPatrolPosition = rightPatrolPosition;

            movementDirection = Vector3.zero;
            lookDirection = Vector3.zero;

            lastShotTime = Time.time;

            currentState = startsActive ? EnemyState.Patrolling : EnemyState.Sleeping;
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

            if (distanceFromPlayer <= detectionRange)
            {
                currentState = EnemyState.Shooting;
                TryShoot();
                return;
            }

            if (currentState == EnemyState.Shooting && distanceFromPlayer >= losePlayerRange)
            {
                currentState = EnemyState.Patrolling;
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

            Debug.Log("Preparing ranged shot...");
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
            if (shootPoint == null || projectilePrefab == null || player == null)
            {
                Debug.LogWarning("Ranged enemy: riferimenti mancanti.");
                return;
            }

            Vector3 targetPosition = player.position + Vector3.up * 1f;
            Vector3 shootDirection = targetPosition - shootPoint.position;

            if (shootDirection.sqrMagnitude < 0.01f)
                return;

            shootDirection.Normalize();

            GameObject projectile = Instantiate(
                projectilePrefab,
                shootPoint.position,
                Quaternion.LookRotation(shootDirection)
            );

            EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();

            if (enemyProjectile == null)
            {
                Debug.LogWarning("Il prefab del proiettile non ha EnemyProjectile.cs.");
                return;
            }

            enemyProjectile.Launch(shootDirection);

            Debug.Log("Ranged enemy shot.");
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

                case EnemyState.Shooting:
                    movementDirection = Vector3.zero;
                    lookDirection = GetPlayerDirection();
                    break;
            }
        }

        private Vector3 GetPatrolDirection()
        {
            if (!hasValidPatrolPoints)
                return Vector3.zero;

            Vector3 directionToTarget = currentPatrolPosition - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.magnitude <= patrolPointTolerance)
                SwitchPatrolTarget();

            return directionToTarget.normalized;
        }

        private void SwitchPatrolTarget()
        {
            float distanceToRight = Vector3.Distance(currentPatrolPosition, rightPatrolPosition);

            if (distanceToRight <= 0.01f)
                currentPatrolPosition = leftPatrolPosition;
            else
                currentPatrolPosition = rightPatrolPosition;
        }

        private Vector3 GetPlayerDirection()
        {
            if (player == null)
                return Vector3.zero;

            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0f;

            return directionToPlayer.normalized;
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetVelocity = movementDirection * patrolSpeed;

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
                Gizmos.DrawSphere(leftPatrolPoint.position, 0.15f);
                Gizmos.DrawSphere(rightPatrolPoint.position, 0.15f);
            }

            if (shootPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
            }
        }
    }
}