using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class BaseEnemyController : MonoBehaviour
    {
        [Header("Base References")]
        [SerializeField] protected CharacterController characterController;

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

        protected Vector3 MovementDirection;
        protected Vector3 LookDirection;

        protected Vector3 SpawnPosition { get; private set; }

        protected EnemyTarget Target => target;
        protected EnemyPatrol Patrol => patrol;

        private Vector3 currentHorizontalVelocity;
        private float verticalVelocity;
        private bool hasResetAfterPlayerDeath;

        protected virtual void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

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

            OnResetToSpawn();
            SetInitialState();
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