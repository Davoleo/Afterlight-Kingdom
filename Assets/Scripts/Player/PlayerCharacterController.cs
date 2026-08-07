using System;
using Gameplay;
using KinematicCharacterController;
using Player.State;
using Projectiles;
using UnityEngine;

namespace Player
{
    public class PlayerCharacterController : MonoBehaviour, ICharacterController
    {
        private ArrowLauncher _arrowLauncher;
        private AbilityManager _abilityManager;

        [Header("References")]
        public KinematicCharacterMotor motor;

        [Header("Jump")]
        [SerializeField] public float jumpUpSpeed = 5f;

        [SerializeField] public float climbJumpStrength = 3f;

        [Header("Dash")]
        [SerializeField] private float dashCooldown = 2f;
        public float dashCooldownTimer;

        [Header("External Knockback")]
        [SerializeField] private float knockbackDrag = 12f;

        private Vector3 externalKnockbackVelocity;

        //avoid knockback cumulation
        private Vector3 appliedExternalKnockbackVelocity;

        private float externalKnockbackTimer;

        public PlayerState CurrentState => StateMachine.CurrentState;
        public bool IsGrounded => motor.GroundingStatus.IsStableOnGround;
        public float ForwardSpeed => Vector3.Dot(motor.Velocity, motor.CharacterForward);
        public float VerticalSpeed => Vector3.Dot(motor.Velocity, motor.CharacterUp);

        public MovementInputs MoveInputs;
        public PlayerCommand commands;

        private Vector3 _currentGroundObjectPos;
        public Vector3 DigitalCharacterForward;

        public Vector3 CurrentLadderNormal;

        public PlayerStateMachine StateMachine;

        private void Awake()
        {
            StateMachine = new PlayerStateMachine(this);

            if (motor == null)
                motor = GetComponent<KinematicCharacterMotor>();

            motor.CharacterController = this;
        }

        private void Start()
        {
            _arrowLauncher = GetComponent<ArrowLauncher>();
            if (_arrowLauncher == null)
                Debug.LogError("ArrowLauncher component missing on " + gameObject.name, this);
            
            _abilityManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AbilityManager>();

            if (_abilityManager == null)
                Debug.LogError("AbilityManager component missing on GameManager", this);

            DigitalCharacterForward = motor.CharacterForward;
        }

        public void SetInputs(MovementInputs inputs, PlayerCommand pcommands)
        {
            MoveInputs = inputs;
            commands |= pcommands;
        }
        
        public void ResetInputs()
        {
            MoveInputs = default;
            commands.Clear();
        }
        
        public void ApplyExternalKnockback(Vector3 direction, float distance, float duration)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            if (duration <= 0f)
                duration = 0.16f;

            direction.Normalize();

            externalKnockbackVelocity = direction * (distance / duration);
            externalKnockbackTimer = duration;
        }

        public void StopExternalKnockback()
        {
            externalKnockbackVelocity = Vector3.zero;
            externalKnockbackTimer = 0f;
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (CurrentState == StateMachine.ClimbingState) return;

            //Rotate player depending on the movement direction
            var movement = ComputeMoveDirection();
            if (movement != Vector3.zero)
            {
                DigitalCharacterForward = movement;
            }

            var newRot = Quaternion.LookRotation(DigitalCharacterForward);
            if (Quaternion.Angle(currentRotation, newRot) > 0.1f)
            {
                currentRotation = Quaternion.Slerp(currentRotation, newRot, deltaTime * 16);
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            //removing previous knockback
            RemovePreviouslyAppliedKnockback(ref currentVelocity);

            CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);

            if (externalKnockbackTimer > 0f)
            {
                currentVelocity += externalKnockbackVelocity;

                appliedExternalKnockbackVelocity = externalKnockbackVelocity;

                externalKnockbackTimer -= deltaTime;

                externalKnockbackVelocity = Vector3.Lerp(
                    externalKnockbackVelocity,
                    Vector3.zero,
                    1f - Mathf.Exp(-knockbackDrag * deltaTime)
                );

                if (externalKnockbackTimer <= 0f)
                {
                    externalKnockbackVelocity = Vector3.zero;
                    externalKnockbackTimer = 0f;
                }
            }
        }        
        private void RemovePreviouslyAppliedKnockback(ref Vector3 currentVelocity)
        {
            if (appliedExternalKnockbackVelocity.sqrMagnitude < 0.01f)
                return;

            Vector3 characterUp = motor.CharacterUp;

            Vector3 currentHorizontalVelocity = Vector3.ProjectOnPlane(
                currentVelocity,
                characterUp
            );

            Vector3 appliedHorizontalKnockback = Vector3.ProjectOnPlane(
                appliedExternalKnockbackVelocity,
                characterUp
            );

            if (appliedHorizontalKnockback.sqrMagnitude < 0.01f)
            {
                appliedExternalKnockbackVelocity = Vector3.zero;
                return;
            }

            Vector3 appliedDirection = appliedHorizontalKnockback.normalized;

            float currentVelocityInKnockbackDirection = Vector3.Dot(
                currentHorizontalVelocity,
                appliedDirection
            );

            if (currentVelocityInKnockbackDirection > 0f)
            {
                float velocityToRemove = Mathf.Min(
                    currentVelocityInKnockbackDirection,
                    appliedHorizontalKnockback.magnitude
                );

                currentVelocity -= appliedDirection * velocityToRemove;
            }

            appliedExternalKnockbackVelocity = Vector3.zero;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            HandleDashInput();

            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - deltaTime);
        }

        private void HandleDashInput()
        {
            if (!CommandUtils.IsUp(commands, PlayerCommand.Dash))
                return;

            if (!_abilityManager.HasAbility(AbilityType.Dash))
            {
                CommandUtils.Off(ref commands, PlayerCommand.Dash);
                return;
            }

            if (dashCooldownTimer > 0f)
            {
                CommandUtils.Off(ref commands, PlayerCommand.Dash);
                return;
            }

            if (CurrentState == StateMachine.DashingState)
            {
                CommandUtils.Off(ref commands, PlayerCommand.Dash);
                return;
            }

            dashCooldownTimer = dashCooldown;
            CommandUtils.Off(ref commands, PlayerCommand.Dash);
            StateMachine.TransitionToState(StateMachine.DashingState);
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            switch (IsGrounded)
            {
                case true when CurrentState == StateMachine.AirborneState:
                    StateMachine.TransitionToState(StateMachine.GroundedState);
                    break;

                case false when CurrentState == StateMachine.GroundedState:
                    StateMachine.TransitionToState(StateMachine.AirborneState);
                    break;
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            CommandUtils.Off(ref commands, PlayerCommand.Jump | PlayerCommand.Dash);

            HandleShootInput();
        }

        private void HandleShootInput()
        {
            if (!CommandUtils.IsUp(commands, PlayerCommand.Shoot))
                return;

            if (!_abilityManager.HasAbility(AbilityType.Bow))
            {
                CommandUtils.Off(ref commands, PlayerCommand.Shoot);
                return;
            }

            _arrowLauncher.TryLaunch(DigitalCharacterForward);

            CommandUtils.Off(ref commands, PlayerCommand.Shoot);
        }

        public Vector3 ComputeMoveDirection()
        {
            if (MoveInputs.MoveInput.sqrMagnitude < 0.01f)
                return Vector3.zero;

            return (MoveInputs.CameraForward * MoveInputs.MoveInput.y
                    + MoveInputs.CameraRight * MoveInputs.MoveInput.x).normalized;
        }

        public void SnapPlayerLocation(float t)
        {
            var pos = transform.position;
            var rX = _currentGroundObjectPos.x;
            var rZ = _currentGroundObjectPos.z;

            var lerpX = Mathf.Lerp(pos.x, rX, t);
            var lerpZ = Mathf.Lerp(pos.z, rZ, t);

            motor.SetPosition(new Vector3(lerpX, pos.y, lerpZ));
        }

        public bool IsColliderValidForCollisions(Collider coll) => true;

        public void OnGroundHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            _currentGroundObjectPos = hitCollider.transform.position;
        }

        public void OnMovementHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            if (externalKnockbackTimer <= 0f)
                return;

            Vector3 horizontalNormal = hitNormal;
            horizontalNormal.y = 0f;

            Vector3 horizontalKnockback = externalKnockbackVelocity;
            horizontalKnockback.y = 0f;

            if (horizontalNormal.sqrMagnitude < 0.01f || horizontalKnockback.sqrMagnitude < 0.01f)
                return;

            horizontalNormal.Normalize();
            horizontalKnockback.Normalize();

            float movingIntoWall = Vector3.Dot(horizontalKnockback, -horizontalNormal);

            if (movingIntoWall > 0.2f)
                StopExternalKnockback();
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
            if (externalKnockbackTimer > 0f)
                StopExternalKnockback();
        }
    }
}