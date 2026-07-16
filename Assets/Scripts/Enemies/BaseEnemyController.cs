using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class BaseEnemyController : MonoBehaviour
    {
        [Header("Base References")]
        [SerializeField] protected CharacterController characterController;
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

        [Header("Base Gravity")]
        [SerializeField] protected float gravity = 20f;
        [SerializeField] protected float groundedGravity = -2f;

        [Header("Base NavMesh")]
        [SerializeField] protected bool useNavMeshPathfinding = false;
        [SerializeField] protected float navMeshRepathInterval = 0.1f;

        protected Vector3 MovementDirection;
        protected Vector3 LookDirection;

        protected Vector3 SpawnPosition { get; private set; }

        protected EnemyTarget Target => target;
        protected EnemyPatrol Patrol => patrol;

        private Vector3 currentHorizontalVelocity;
        private float verticalVelocity;
        private bool hasResetAfterPlayerDeath;
        private float nextNavMeshRepathTime;

        protected virtual void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (navMeshAgent == null)
                navMeshAgent = GetComponent<NavMeshAgent>();

            if (navMeshAgent != null)
            {
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
            }

            if (target == null)
                target = new EnemyTarget();

            if (patrol == null)
                patrol = new EnemyPatrol();
        }

        protected virtual void Start()
        {
            SpawnPosition = transform.position;

            target.Initialize();
            patrol.Initialize(SpawnPosition);

            SyncNavMeshAgentPosition();

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
            RotateTowardsLookDirection(deltaTime);
            MoveCharacter(deltaTime);
        }

        private void MoveCharacter(float deltaTime)
        {
            Vector3 targetHorizontalVelocity = MovementDirection * GetCurrentSpeed();

            currentHorizontalVelocity = Vector3.Lerp(
                currentHorizontalVelocity,
                targetHorizontalVelocity,
                1f - Mathf.Exp(-acceleration * deltaTime)
            );

            ApplyGravity(deltaTime);

            Vector3 finalVelocity = currentHorizontalVelocity;
            finalVelocity.y = verticalVelocity;

            characterController.Move(finalVelocity * deltaTime);

            SyncNavMeshAgentPosition();
        }

        private void ApplyGravity(float deltaTime)
        {
            if (characterController.isGrounded)
            {
                if (verticalVelocity < 0f)
                    verticalVelocity = groundedGravity;
            }
            else
            {
                verticalVelocity -= gravity * deltaTime;
            }
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
            characterController.enabled = false;
            transform.position = SpawnPosition;
            characterController.enabled = true;

            patrol.Reset();

            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            currentHorizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            nextNavMeshRepathTime = 0f;

            ResetNavMeshAgentPath();
            SyncNavMeshAgentPosition();

            OnResetToSpawn();
            SetInitialState();
        }

        protected Vector3 GetNavMeshDirectionTo(Vector3 destination)
        {
            if (!useNavMeshPathfinding)
                return Vector3.zero;

            if (navMeshAgent == null)
                return Vector3.zero;

            if (!navMeshAgent.enabled)
                return Vector3.zero;

            if (!navMeshAgent.isOnNavMesh)
                return Vector3.zero;

            navMeshAgent.speed = GetCurrentSpeed();
            navMeshAgent.acceleration = acceleration;
            navMeshAgent.nextPosition = transform.position;

            if (Time.time >= nextNavMeshRepathTime)
            {
                bool destinationSet = navMeshAgent.SetDestination(destination);

                if (!destinationSet)
                    return Vector3.zero;

                nextNavMeshRepathTime = Time.time + navMeshRepathInterval;
            }

            Vector3 desiredVelocity = navMeshAgent.desiredVelocity;
            desiredVelocity.y = 0f;

            if (desiredVelocity.sqrMagnitude >= 0.01f)
                return desiredVelocity.normalized;

            Vector3 directionToSteeringTarget = navMeshAgent.steeringTarget - transform.position;
            directionToSteeringTarget.y = 0f;

            if (directionToSteeringTarget.sqrMagnitude < 0.01f)
                return Vector3.zero;

            return directionToSteeringTarget.normalized;
        }

        protected Vector3 GetNavMeshPatrolDirection()
        {
            patrol.UpdateTargetIfReached(transform.position);

            Vector3 destination = patrol.GetCurrentTargetPosition();

            Vector3 navMeshDirection = GetNavMeshDirectionTo(destination);

            if (navMeshDirection.sqrMagnitude < 0.01f)
                return GetPatrolDirection();

            return navMeshDirection;
        }

        private void SyncNavMeshAgentPosition()
        {
            if (navMeshAgent == null)
                return;

            if (!navMeshAgent.enabled)
                return;

            if (!navMeshAgent.isOnNavMesh)
                return;

            navMeshAgent.nextPosition = transform.position;
        }

        private void ResetNavMeshAgentPath()
        {
            if (navMeshAgent == null)
                return;

            if (!navMeshAgent.enabled)
                return;

            if (!navMeshAgent.isOnNavMesh)
                return;

            navMeshAgent.ResetPath();
            navMeshAgent.nextPosition = transform.position;
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

        protected Vector3 GetPatrolDirection()
        {
            return patrol.GetDirection(transform.position);
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

        protected bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        protected bool IsOwnCollider(Collider colliderToCheck)
        {
            return colliderToCheck != null
                   && (colliderToCheck.transform == transform
                       || colliderToCheck.transform.IsChildOf(transform));
        }

        protected virtual void OnControllerColliderHit(ControllerColliderHit hit) { }

        protected virtual void OnDrawGizmosSelected()
        {
            target.DrawGizmos(transform.position);
            patrol.DrawGizmos();
        }
    }
}