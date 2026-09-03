using System;
using Core;
using Gameplay;
using HUD.Assist;
using KinematicCharacterController;
using Player.State;
using UnityEngine;

namespace Player
{
    public class PlayerCharacterController : MonoBehaviour, ICharacterController
    {
        private AbilityManager _abilityManager;
        private AssistEvents _assistEvents;

        [Header("References")]
        public KinematicCharacterMotor motor;

        [Header("Jump")]
        [SerializeField] public float jumpUpSpeed = 5f;

        [SerializeField] public float climbJumpStrength = 3f;

        [Tooltip("Grace period after leaving the ground (without jumping) during which a jump is still allowed.")]
        [SerializeField] private float coyoteTime = 0.12f;

        private float _coyoteTimer;

        // True while the player just walked off a ledge and is still inside the coyote window.
        public bool CanCoyoteJump => _coyoteTimer > 0f;

        // Closes the coyote window so a single press can never produce more than one jump.
        public void ConsumeCoyote() => _coyoteTimer = 0f;

        [Header("Dash")]
        [SerializeField] private Cooldown dashCooldown = new(2f);
        public Cooldown DashCooldown => dashCooldown;

        [Header("External Knockback")]
        [SerializeField] private float knockbackDrag = 12f;

        [Header("Enemy Collision")]
        [SerializeField] private float enemyTopSlideSpeed = 3f;

        private Vector3 externalKnockbackVelocity;

        //avoid knockback cumulation
        private Vector3 appliedExternalKnockbackVelocity;

        private float externalKnockbackTimer;

        public PlayerState CurrentState => StateMachine.CurrentState;
        public bool IsGrounded => motor.GroundingStatus.IsStableOnGround;
        public float ForwardSpeed => Vector3.Dot(motor.Velocity, motor.CharacterForward);
        public float VerticalSpeed => Vector3.Dot(motor.Velocity, motor.CharacterUp);

        public PlayerInputs PlayerInputs;
        public PlayerTrigger triggers;

        [NonSerialized]
        public GameObject CurrentGroundObject;
        [NonSerialized]
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
            var gm = GameObject.FindGameObjectWithTag("GameManager");
            _abilityManager = gm.GetComponent<AbilityManager>();
            _assistEvents = gm.GetComponent<AssistEvents>();

            if (_abilityManager == null)
                Debug.LogError("AbilityManager component missing on GameManager", this);

            DigitalCharacterForward = motor.CharacterForward;
        }

        public void SetInputs(PlayerInputs inputs, PlayerTrigger pcommands)
        {
            PlayerInputs = inputs;
            triggers |= pcommands;
        }
        
        public void ResetInputs()
        {
            PlayerInputs = default;
            triggers.Clear();
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

            dashCooldown.Tick(deltaTime);

            // Refill the window every grounded frame; drain it once airborne. A jump that
            // originates from the ground calls ConsumeCoyote() to zero it out immediately,
            // so this only stays positive when the player leaves the ground by walking off.
            _coyoteTimer = IsGrounded ? coyoteTime : _coyoteTimer - deltaTime;
        }

        private void HandleDashInput()
        {
            if (!CommandUtils.IsUp(triggers, PlayerTrigger.Dash))
                return;

            if (!_abilityManager.HasAbility(AbilityType.Dash))
            {
                CommandUtils.Off(ref triggers, PlayerTrigger.Dash);
                return;
            }

            if (!dashCooldown.IsReady)
            {
                CommandUtils.Off(ref triggers, PlayerTrigger.Dash);
                return;
            }

            //Disable dash hints
            _assistEvents.OnPlayerDash();

            if (CurrentState == StateMachine.DashingState)
            {
                CommandUtils.Off(ref triggers, PlayerTrigger.Dash);
                return;
            }

            dashCooldown.Trigger();
            CommandUtils.Off(ref triggers, PlayerTrigger.Dash);
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
            CommandUtils.Off(ref triggers, PlayerTrigger.Interact | PlayerTrigger.Dash | PlayerTrigger.Jump);
        }

        public Vector3 ComputeMoveDirection()
        {
            if (Mathf.Abs(PlayerInputs.MoveInput) < 0.01f)
                return Vector3.zero;

            return PlayerInputs.CameraRight * PlayerInputs.MoveInput;
        }

        public void SnapPlayerLocation(float t)
        {
            var pos = transform.position;
            var rX = CurrentGroundObject.transform.position.x;
            var rZ = CurrentGroundObject.transform.position.z;

            var lerpX = Mathf.Lerp(pos.x, rX, t);
            var lerpZ = Mathf.Lerp(pos.z, rZ, t);

            motor.SetPosition(new Vector3(lerpX, pos.y, lerpZ));
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            //Layer 3 = Arrow
            if (coll.gameObject.layer == 3)
            {
                //Debug.Log(coll.bounds.max.y + " " + motor.Capsule.bounds.min.y);

                if (motor.Capsule.bounds.min.y < coll.bounds.max.y - 0.05f)
                {
                    return false;
                }
            }

            return true;
        }

        public void OnGroundHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            CurrentGroundObject = hitCollider.gameObject;
        }

        public void OnMovementHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            //if the player lands on top of an enemy, force the horizontal movement only backwards so it cannot slide sideways
            if (hitCollider.CompareTag("Enemy") && hitNormal.y > 0.5f)
            {
                Vector3 backwardDirection = -DigitalCharacterForward;
                backwardDirection.y = 0f;

                if (backwardDirection.sqrMagnitude > 0.01f)
                {
                    backwardDirection.Normalize();
                    motor.BaseVelocity = new Vector3(backwardDirection.x * enemyTopSlideSpeed,motor.BaseVelocity.y,backwardDirection.z * enemyTopSlideSpeed);
                }
            }
            //if the player is moving directly into an enemy or its blocker, stop the horizontal movement so it does not redirect the player sideways, avoiding lateralslides
            else if (hitCollider.CompareTag("Enemy"))
            {
                Vector3 horizontalVelocity = motor.Velocity;
                horizontalVelocity.y = 0f;

                Vector3 horizontalNormal = hitNormal;
                horizontalNormal.y = 0f;

                if (horizontalVelocity.sqrMagnitude > 0.01f && horizontalNormal.sqrMagnitude > 0.01f)
                {
                    horizontalVelocity.Normalize();
                    horizontalNormal.Normalize();
                    float movingIntoEnemy = Vector3.Dot(horizontalVelocity, -horizontalNormal);

                    if (movingIntoEnemy > 0.2f)
                    {
                        motor.BaseVelocity = new Vector3(0f,motor.BaseVelocity.y,0f);
                    }
                }
            }

            if (externalKnockbackTimer <= 0f)
                return;

            Vector3 horizontalNormalKnockback = hitNormal;
            horizontalNormalKnockback.y = 0f;

            Vector3 horizontalKnockback = externalKnockbackVelocity;
            horizontalKnockback.y = 0f;

            if (horizontalNormalKnockback.sqrMagnitude < 0.01f || horizontalKnockback.sqrMagnitude < 0.01f)
                return;

            horizontalNormalKnockback.Normalize();
            horizontalKnockback.Normalize();

            float movingIntoWall = Vector3.Dot(horizontalKnockback, -horizontalNormalKnockback);

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
        {
            if (hitCollider.CompareTag("Enemy"))
                hitStabilityReport.IsStable = false;
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            if (externalKnockbackTimer > 0f)
                StopExternalKnockback();
        }
    }
}