using KinematicCharacterController;
using Player;
using UnityEngine;

namespace Enemies
{
    public class ChargingEnemyController : MonoBehaviour, ICharacterController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            PreparingCharge,
            Charging,
            Cooldown
        }

        [Header("References")]
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private Transform player;
        [SerializeField] private Transform leftPatrolPoint;
        [SerializeField] private Transform rightPatrolPoint;
        [SerializeField] private Transform chargeHitPoint;

        [Header("Activation")]
        [SerializeField] private bool startsActive = true;
        [SerializeField] private float detectionRange = 7f;
        [SerializeField] private float losePlayerRange = 10f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chargeSpeed = 9f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float patrolPointTolerance = 0.25f;

        [Header("Charge")]
        [SerializeField] private float chargeWindup = 0.45f;
        [SerializeField] private float chargeDuration = 0.75f;
        [SerializeField] private float chargeCooldown = 1.2f;
        [SerializeField] private float chargeHitRadius = 1.1f;
        [SerializeField] private int chargeDamage = 1;
        [SerializeField] private float chargeKnockbackDistance = 4f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float obstacleCheckDistance = 0.25f;

        [Header("Gravity")]
        [SerializeField] private float gravity = 20f;

        private EnemyState currentState;

        private Vector3 spawnPosition;
        private Vector3 movementDirection;
        private Vector3 lookDirection;
        private Vector3 chargeDirection;

        private Vector3 leftPatrolPosition;
        private Vector3 rightPatrolPosition;
        private Vector3 currentPatrolPosition;

        private bool hasValidPatrolPoints;
        private bool hasHitPlayerDuringCharge;
        private float stateStartTime;

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
        }

        public void Activate()
        {
            if (currentState == EnemyState.Sleeping)
                ChangeState(EnemyState.Patrolling);
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
            if (motor != null)
                motor.SetPosition(spawnPosition);
            else
                transform.position = spawnPosition;

            currentPatrolPosition = rightPatrolPosition;

            movementDirection = Vector3.zero;
            lookDirection = Vector3.zero;
            chargeDirection = Vector3.zero;

            hasHitPlayerDuringCharge = false;

            ChangeState(startsActive ? EnemyState.Patrolling : EnemyState.Sleeping);
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

            switch (currentState)
            {
                case EnemyState.Patrolling:
                    if (distanceFromPlayer <= detectionRange)
                        PrepareCharge();
                    break;

                case EnemyState.PreparingCharge:
                    if (distanceFromPlayer >= losePlayerRange)
                    {
                        ChangeState(EnemyState.Patrolling);
                        return;
                    }

                    if (Time.time >= stateStartTime + chargeWindup)
                        StartCharge();

                    break;

                case EnemyState.Charging:
                    CheckChargeHit();

                    if (IsObstacleInChargeDirection())
                    {
                        StopChargeAgainstObstacle();
                        return;
                    }

                    if (Time.time >= stateStartTime + chargeDuration)
                        ChangeState(EnemyState.Cooldown);

                    break;

                case EnemyState.Cooldown:
                    if (Time.time >= stateStartTime + chargeCooldown)
                    {
                        if (distanceFromPlayer <= detectionRange)
                            PrepareCharge();
                        else
                            ChangeState(EnemyState.Patrolling);
                    }

                    break;
            }
        }

        private void PrepareCharge()
        {
            chargeDirection = GetPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
                return;

            movementDirection = Vector3.zero;
            lookDirection = chargeDirection;
            hasHitPlayerDuringCharge = false;

            ChangeState(EnemyState.PreparingCharge);

            Debug.Log("Charging enemy preparing charge.");
        }

        private void StartCharge()
        {
            chargeDirection = GetPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
            {
                ChangeState(EnemyState.Patrolling);
                return;
            }

            if (IsObstacleInChargeDirection())
            {
                StopChargeAgainstObstacle();
                return;
            }

            movementDirection = chargeDirection;
            lookDirection = chargeDirection;
            hasHitPlayerDuringCharge = false;

            ChangeState(EnemyState.Charging);

            Debug.Log("Charging enemy started charge.");
        }

        private bool IsObstacleInChargeDirection()
        {
            if (chargeDirection.sqrMagnitude < 0.01f)
                return false;

            Vector3 origin = motor != null
                ? motor.TransientPosition + Vector3.up * 0.6f
                : transform.position + Vector3.up * 0.6f;

            float radius = motor != null ? motor.Capsule.radius * 0.95f : 0.35f;

            return Physics.SphereCast(
                origin,
                radius,
                chargeDirection.normalized,
                out RaycastHit hit,
                obstacleCheckDistance,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );
        }

        private void StopChargeAgainstObstacle()
        {
            movementDirection = Vector3.zero;
            chargeDirection = Vector3.zero;

            ChangeState(EnemyState.Cooldown);

            Debug.Log("Charging enemy stopped against obstacle.");
        }

        private void CheckChargeHit()
        {
            if (hasHitPlayerDuringCharge)
                return;

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + chargeDirection * 0.9f + Vector3.up * 0.7f;

            Collider[] hitColliders = Physics.OverlapSphere(
                hitPosition,
                chargeHitRadius,
                playerLayer
            );

            foreach (Collider hitCollider in hitColliders)
            {
                PlayerDamageFeedback damageFeedback =
                    hitCollider.GetComponentInParent<PlayerDamageFeedback>();

                if (damageFeedback != null)
                {
                    bool damageApplied = damageFeedback.TryTakeDamage(
                        chargeDamage,
                        chargeDirection,
                        true,
                        chargeKnockbackDistance
                    );

                    if (damageApplied)
                    {
                        hasHitPlayerDuringCharge = true;
                        Debug.Log("Charging enemy hit player.");
                    }
                    else
                    {
                        Debug.Log("Charging hit ignored: player invincible.");
                    }

                    return;
                }

                HealthManager healthManager =
                    hitCollider.GetComponentInParent<HealthManager>();

                if (healthManager != null)
                {
                    healthManager.TakeDamage(chargeDamage);
                    hasHitPlayerDuringCharge = true;

                    Debug.LogWarning("PlayerDamageFeedback non trovato, usato HealthManager diretto.");

                    return;
                }
            }
        }

        private void ChangeState(EnemyState newState)
        {
            currentState = newState;
            stateStartTime = Time.time;
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

                case EnemyState.PreparingCharge:
                    movementDirection = Vector3.zero;
                    lookDirection = chargeDirection.sqrMagnitude > 0.01f
                        ? chargeDirection
                        : GetPlayerDirection();
                    break;

                case EnemyState.Charging:
                    movementDirection = chargeDirection;
                    lookDirection = chargeDirection;
                    break;

                case EnemyState.Cooldown:
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
            float speed = currentState == EnemyState.Charging ? chargeSpeed : patrolSpeed;
            Vector3 targetVelocity = movementDirection * speed;

            if (currentState == EnemyState.Charging && IsObstacleInChargeDirection())
            {
                currentVelocity = Vector3.zero;
                StopChargeAgainstObstacle();
                return;
            }

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
        {
            if (currentState != EnemyState.Charging)
                return;

            if (((1 << hitCollider.gameObject.layer) & obstacleLayer) == 0)
                return;

            StopChargeAgainstObstacle();
        }

        public void ProcessHitStabilityReport(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation,
            ref HitStabilityReport hitStabilityReport)
        { }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            if (currentState != EnemyState.Charging)
                return;

            if (((1 << hitCollider.gameObject.layer) & obstacleLayer) == 0)
                return;

            StopChargeAgainstObstacle();
        }

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

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + transform.forward * 0.9f + Vector3.up * 0.7f;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitPosition, chargeHitRadius);
        }
    }
}