using KinematicCharacterController;
using UnityEngine;

namespace Enemies
{
    public abstract class EnemyController : MonoBehaviour, ICharacterController
    {
        private bool hasResetAfterPlayerDeath;

        [Header("Base References")]
        [SerializeField] protected KinematicCharacterMotor motor;

        [Header("Common Target")]
        [SerializeField] protected EnemyTarget target = new EnemyTarget();

        [Header("Common Patrol")]
        [SerializeField] protected EnemyPatrol patrol = new EnemyPatrol();

        [Header("Base Activation")]
        [SerializeField] protected bool startsActive = true;

        [Header("Base Movement")]
        [SerializeField] protected float patrolSpeed = 2f;
        [SerializeField] protected float acceleration = 15f;

        [Header("Base Gravity")]
        [SerializeField] protected float gravity = 20f;

        protected Vector3 SpawnPosition;
        protected Vector3 MovementDirection;
        protected Vector3 LookDirection;

        protected virtual void Awake()
        {
            if (motor == null)
                motor = GetComponent<KinematicCharacterMotor>();

            if (motor != null)
                motor.CharacterController = this;
        }

        protected virtual void Start()
        {
            SpawnPosition = transform.position;

            target.FindPlayer();
            patrol.Initialize(transform.position);

            SetInitialState();
        }

        protected virtual void Update()
        {
            if (CheckPlayerDeathOrFall())
                return;

            UpdateEnemy();
            UpdateMovementDirection();
        }

        public abstract void Activate();

        protected abstract void SetInitialState();
        protected abstract void UpdateEnemy();
        protected abstract void UpdateMovementDirection();
        protected abstract void ResetSpecificState();
        protected abstract void SetStateAfterReset();

        protected virtual float GetCurrentSpeed()
        {
            return patrolSpeed;
        }

        protected virtual void ResetToSpawn()
        {
            ResetSpecificState();

            if (motor != null)
                motor.SetPosition(SpawnPosition);
            else
                transform.position = SpawnPosition;

            patrol.Reset();

            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            SetStateAfterReset();
        }
        //manage enemies respawn
        protected bool CheckPlayerDeathOrFall()
        {
            if (!target.IsPlayerDead())
            {
                hasResetAfterPlayerDeath = false;
                return false;
            }

            if (hasResetAfterPlayerDeath)
                return true;

            hasResetAfterPlayerDeath = true;
            ResetToSpawn();

            return true;
        }

        protected bool IsPlayerDead()
        {
            return target.IsPlayerDead();
        }
        //classe che gestisce la
        protected float DistanceFromPlayer()
        {
            return target.DistanceFrom(transform.position);
        }

        protected Vector3 GetPlayerDirection()
        {
            return target.DirectionFrom(transform.position);
        }

        protected Vector3 GetPlayerAimPosition(float verticalOffset = 1f)
        {
            return target.AimPosition(verticalOffset);
        }

        protected Transform PlayerTransform()
        {
            return target.Player;
        }

        protected Vector3 GetPatrolDirection()
        {
            return patrol.GetDirection(transform.position);
        }
        //modify speed parameters when switch from patrol to attack mode
        public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetVelocity = MovementDirection * GetCurrentSpeed();

            if (motor != null && motor.GroundingStatus.IsStableOnGround)
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

        public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (LookDirection.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(LookDirection, Vector3.up);

            currentRotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                1f - Mathf.Exp(-acceleration * deltaTime)
            );
        }

        public virtual void BeforeCharacterUpdate(float deltaTime) { }

        public virtual void PostGroundingUpdate(float deltaTime) { }

        public virtual void AfterCharacterUpdate(float deltaTime) { }

        public virtual bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public virtual void OnGroundHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        { }

        public virtual void OnMovementHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        { }

        public virtual void ProcessHitStabilityReport(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation,
            ref HitStabilityReport hitStabilityReport)
        { }

        public virtual void OnDiscreteCollisionDetected(Collider hitCollider) { }

        protected virtual void OnDrawGizmosSelected()
        {
            target.DrawGizmos(transform.position);
            patrol.DrawGizmos();
        }
    }
}