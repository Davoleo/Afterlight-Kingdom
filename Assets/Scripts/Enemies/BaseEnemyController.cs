using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseEnemyController : MonoBehaviour
    {
        [Header("Base References")]
        [SerializeField] protected NavMeshAgent navMeshAgent;

        [Header("Base Components")]
        [SerializeField] protected EnemyTarget target = new EnemyTarget();
        [SerializeField] protected EnemyPatrol patrol = new EnemyPatrol();

        [Header("Base Activation")]
        [SerializeField] protected bool startsActive = true;

        [Header("Base Movement")]
        [SerializeField] protected float patrolSpeed = 2f;
        [SerializeField] protected float acceleration = 15f;
        [SerializeField] protected float rotationSpeed = 12f;

        [Header("Base NavMesh")]
        [SerializeField] protected float navMeshRepathInterval = 0.1f;
        [SerializeField] protected float navMeshSampleRadius = 2f;
        [SerializeField] protected float navMeshStoppingDistance = 0.05f;

        protected Vector3 MovementDirection;
        protected Vector3 LookDirection;

        protected Vector3 SpawnPosition { get; private set; }

        protected EnemyTarget Target => target;

        private bool hasResetAfterPlayerDeath;
        private bool hasNavMeshDestination;

        private float nextNavMeshRepathTime;
        private Vector3 currentNavMeshDestination;

        protected virtual void Awake()
        {
            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();

            // The NavMeshAgent is the only component that updates the enemy position.
            navMeshAgent.updatePosition = true;

            // Rotation is handled manually because some enemies must look at the player
            // while remaining stationary.
            navMeshAgent.updateRotation = false;

            if (target == null)
                target = new EnemyTarget();

            if (patrol == null)
                patrol = new EnemyPatrol();
        }

        protected virtual void Start()
        {
            target.Initialize();

            if (!TryPlaceAgentOnNavMesh(transform.position))
            {
                Debug.LogError(
                    $"{name}: the enemy is not placed on a valid NavMesh.",
                    this
                );

                enabled = false;
                return;
            }

            // The NavMesh can slightly adjust the initial position.
            SpawnPosition = transform.position;

            patrol.Initialize(SpawnPosition);

            SetInitialState();
        }

        protected virtual void Update()
        {
            HandlePlayerDeathReset();
        }

        public abstract void Activate();

        protected abstract void SetInitialState();

        protected virtual void OnResetToSpawn() { }

        protected virtual float GetCurrentSpeed()
        {
            return patrolSpeed;
        }

        protected void MoveAndRotate(float deltaTime)
        {
            MoveCharacter(deltaTime);
            RotateTowardsLookDirection(deltaTime);
        }

        private void MoveCharacter(float deltaTime)
        {
            if (!IsNavMeshAgentReady())
                return;

            navMeshAgent.speed = GetCurrentSpeed();
            navMeshAgent.acceleration = acceleration;
            navMeshAgent.stoppingDistance = navMeshStoppingDistance;

            // The agent must remain active while traversing a NavMesh Link,
            // even if its desired direction briefly becomes zero.
            bool shouldMove =
                navMeshAgent.isOnOffMeshLink
                || (
                    hasNavMeshDestination
                    && MovementDirection.sqrMagnitude >= 0.01f
                );

            navMeshAgent.isStopped = !shouldMove;
        }

        private void RotateTowardsLookDirection(float deltaTime)
        {
            if (LookDirection.sqrMagnitude < 0.01f)
                return;

            Vector3 flatLookDirection = LookDirection;
            flatLookDirection.y = 0f;

            if (flatLookDirection.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(
                flatLookDirection.normalized,
                Vector3.up
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-rotationSpeed * deltaTime)
            );
        }

        private void HandlePlayerDeathReset()
        {
            if (!target.IsPlayerDead())
            {
                hasResetAfterPlayerDeath = false;
                return;
            }

            if (hasResetAfterPlayerDeath)
                return;

            ResetToSpawn();
            hasResetAfterPlayerDeath = true;
        }

        protected void ResetToSpawn()
        {
            ResetNavMeshAgentPath();

            if (TrySampleNavMeshPosition(
                    SpawnPosition,
                    out Vector3 resetPosition))
            {
                // Warp moves the enemy through the NavMeshAgent
                // instead of modifying transform.position directly.
                if (!navMeshAgent.Warp(resetPosition))
                {
                    Debug.LogWarning(
                        $"{name}: the NavMeshAgent could not return to its spawn position.",
                        this
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    $"{name}: the spawn position is not close to a valid NavMesh.",
                    this
                );
            }

            patrol.Reset();

            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            nextNavMeshRepathTime = 0f;

            ResetNavMeshAgentPath();

            OnResetToSpawn();
            SetInitialState();
        }

        protected Vector3 GetNavMeshDirectionTo(Vector3 destination)
        {
            if (!IsNavMeshAgentReady())
                return Vector3.zero;

            if (!TrySampleNavMeshPosition(
                    destination,
                    out Vector3 sampledDestination))
            {
                return Vector3.zero;
            }

            bool destinationChanged =
                !hasNavMeshDestination
                || Vector3.SqrMagnitude(
                    sampledDestination - currentNavMeshDestination
                ) > 0.0625f;

            if (destinationChanged || Time.time >= nextNavMeshRepathTime)
            {
                if (!navMeshAgent.SetDestination(sampledDestination))
                {
                    hasNavMeshDestination = false;
                    return Vector3.zero;
                }

                currentNavMeshDestination = sampledDestination;
                hasNavMeshDestination = true;
                nextNavMeshRepathTime =
                    Time.time + navMeshRepathInterval;
            }

            if (navMeshAgent.pathPending)
                return GetFlatDirectionTo(sampledDestination);

            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                hasNavMeshDestination = false;
                return Vector3.zero;
            }

            Vector3 desiredVelocity = navMeshAgent.desiredVelocity;
            desiredVelocity.y = 0f;

            if (desiredVelocity.sqrMagnitude >= 0.01f)
                return desiredVelocity.normalized;

            Vector3 steeringDirection =
                navMeshAgent.steeringTarget - transform.position;

            steeringDirection.y = 0f;

            if (steeringDirection.sqrMagnitude >= 0.01f)
                return steeringDirection.normalized;

            return GetFlatDirectionTo(sampledDestination);
        }

        protected Vector3 GetNavMeshPatrolDirection()
        {
            patrol.UpdateTargetIfReached(transform.position);

            Vector3 destination =
                patrol.GetCurrentTargetPosition();

            return GetNavMeshDirectionTo(destination);
        }

        protected bool TrySampleNavMeshPosition(
            Vector3 desiredPosition,
            out Vector3 sampledPosition)
        {
            return TrySampleNavMeshPosition(
                desiredPosition,
                navMeshSampleRadius,
                out sampledPosition
            );
        }

        protected bool TrySampleNavMeshPosition(
            Vector3 desiredPosition,
            float sampleRadius,
            out Vector3 sampledPosition)
        {
            sampledPosition = desiredPosition;

            int areaMask = navMeshAgent != null
                ? navMeshAgent.areaMask
                : NavMesh.AllAreas;

            bool positionFound = NavMesh.SamplePosition(
                desiredPosition,
                out NavMeshHit hit,
                sampleRadius,
                areaMask
            );

            if (!positionFound)
                return false;

            sampledPosition = hit.position;
            return true;
        }

        private bool TryPlaceAgentOnNavMesh(Vector3 desiredPosition)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled)
                return false;

            if (navMeshAgent.isOnNavMesh)
                return true;

            if (!TrySampleNavMeshPosition(
                    desiredPosition,
                    out Vector3 sampledPosition))
            {
                return false;
            }

            return navMeshAgent.Warp(sampledPosition);
        }

        private bool IsNavMeshAgentReady()
        {
            return navMeshAgent != null
                   && navMeshAgent.enabled
                   && navMeshAgent.isOnNavMesh;
        }

        private Vector3 GetFlatDirectionTo(Vector3 destination)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return Vector3.zero;

            return direction.normalized;
        }

        private void ResetNavMeshAgentPath()
        {
            hasNavMeshDestination = false;
            currentNavMeshDestination = Vector3.zero;

            if (!IsNavMeshAgentReady())
                return;

            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        protected bool HasPlayer()
        {
            return target.HasPlayer();
        }

        protected bool IsPlayerDead()
        {
            return target.IsPlayerDead();
        }

        protected float GetDistanceFromPlayer()
        {
            return target.DistanceFrom(transform.position);
        }

        protected bool IsPlayerInsideDetection()
        {
            return target.IsInsideDetection(transform.position);
        }

        protected bool IsPlayerOutsideLoseRange()
        {
            return target.IsOutsideLoseRange(transform.position);
        }

        protected Vector3 GetPlayerDirection()
        {
            return target.DirectionFrom(transform.position);
        }

        protected Vector3 GetPlayerAimPosition(float verticalOffset = 1f)
        {
            return target.AimPosition(verticalOffset);
        }

        protected bool TryDamagePlayer(
            Collider hitCollider,
            int damage,
            Vector3 knockbackDirection,
            bool useKnockback,
            float knockbackDistance = 0f)
        {
            return EnemyPlayerDamage.TryDamage(
                hitCollider,
                damage,
                knockbackDirection,
                useKnockback,
                knockbackDistance
            );
        }

        protected bool IsOwnCollider(Collider colliderToCheck)
        {
            return colliderToCheck != null
                   && (
                       colliderToCheck.transform == transform
                       || colliderToCheck.transform.IsChildOf(transform)
                   );
        }

        protected virtual void OnDrawGizmosSelected()
        {
            target.DrawGizmos(transform.position);
            patrol.DrawGizmos();
        }
    }
}