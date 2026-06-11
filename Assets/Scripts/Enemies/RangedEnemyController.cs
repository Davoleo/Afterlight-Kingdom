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
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float losePlayerRange = 10f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float patrolPointTolerance = 0.25f;

        [Header("Shooting")]
        [SerializeField] private float shootCooldown = 1.5f;
        [SerializeField] private float shootWindup = 0.3f;

        [Header("Gravity")]
        [SerializeField] private float gravity = 20f;

        private EnemyState currentState;
        private Transform currentPatrolTarget;

        private Vector3 spawnPosition;
        private Vector3 movementDirection;
        private Vector3 lookDirection;

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
            UpdateShot();
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
            isPreparingShot = false;

            if (motor != null)
                motor.SetPosition(spawnPosition);
            else
                transform.position = spawnPosition;

            currentState = startsActive ? EnemyState.Patrolling : EnemyState.Sleeping;
            currentPatrolTarget = rightPatrolPoint;

            movementDirection = Vector3.zero;
            lookDirection = Vector3.zero;

            lastShotTime = Time.time;
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
            if (shootPoint == null)
            {
                Debug.LogWarning("Ranged enemy: ShootPoint mancante.");
                return;
            }

            if (projectilePrefab == null)
            {
                Debug.LogWarning("Ranged enemy: Projectile Prefab mancante.");
                return;
            }

            if (player == null)
            {
                Debug.LogWarning("Ranged enemy: Player mancante.");
                return;
            }

            Vector3 targetPosition = player.position + Vector3.up * 1f;
            Vector3 direction = targetPosition - shootPoint.position;

            if (direction.sqrMagnitude < 0.01f)
                return;

            direction.Normalize();

            GameObject projectile = Instantiate(
                projectilePrefab,
                shootPoint.position,
                Quaternion.LookRotation(direction)
            );

            EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();

            if (enemyProjectile == null)
            {
                Debug.LogWarning("Il prefab del proiettile non ha EnemyProjectile.cs.");
                return;
            }

            enemyProjectile.Launch(direction);

            Debug.Log("Ranged enemy shot toward player.");
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
            }

            if (shootPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
            }
        }
    }
}