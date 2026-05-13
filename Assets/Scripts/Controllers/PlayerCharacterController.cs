using System;
using KinematicCharacterController;
using Player;
using Projectiles;
using UnityEngine;

namespace Controllers
{
    /// <summary>
    /// Implements ICharacterController so KinematicCharacterMotor calls us
    /// once per FixedUpdate with the correct callback order:
    ///
    ///   BeforeCharacterUpdate  → read latched inputs, trigger state transitions
    ///   UpdateRotation         → tell the motor where to face
    ///   UpdateVelocity         → tell the motor how fast to move
    ///   PostGroundingUpdate    → react to landing / leaving ground
    ///   AfterCharacterUpdate   → consume one-shot flags
    ///
    /// Rule: velocity and rotation are ONLY ever set inside their respective
    /// callbacks. Never call motor.Move() yourself.
    ///
    /// Public read-only properties (IsGrounded, MovementMagnitude, CurrentState)
    /// are the interface used by a future PlayerAnimationController.
    /// </summary>
    public class PlayerCharacterController : MonoBehaviour, ICharacterController
    {
        private ArrowLauncher _arrowLauncher;
        [Header("References")]
        public KinematicCharacterMotor motor;

        [Header("Ground Movement")]
        [SerializeField] private float maxMoveSpeed       = 5f;
        [SerializeField] private float movementSharpness  = 15f;    // higher = snappier acceleration

        [Header("Air Movement")] 
        [SerializeField] private float maxAirMoveSpeed  = 5f;
        [SerializeField] private float airAcceleration  = 5f;
        [SerializeField] private Vector3 gravity        = new Vector3(0f, -20f, 0f);

        [Header("Jump")]
        [SerializeField] private float jumpUpSpeed = 5f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed    = 30f;
        [SerializeField] public float dashDuration = 0.2f; // seconds
        [SerializeField] private float dashCooldown = 2f;   // seconds
        public float   dashDurationTimer;
        public float   dashCooldownTimer;
        public Vector3 dashDirection;

        [Header("Rotation")]
        [SerializeField] private float stepAngle        = 90f;
        [SerializeField] private float rotationDuration = 0.3f;

        // ── Public state (consumed by PlayerAnimationController) ─────────────────
        public event Action OnJumped;
        public CharacterState CurrentState => StateMachine.CurrentState;
        public bool  IsGrounded    => motor.GroundingStatus.IsStableOnGround;
        public float ForwardSpeed  => Vector3.Dot(motor.Velocity, motor.CharacterForward);
        public float VerticalSpeed => Vector3.Dot(motor.Velocity, motor.CharacterUp);

        // ── Input cache ───────────────────────────────────────────────────────────
        private PlayerInputs _inputs;

        // Latched flags: consumed in callbacks (FixedUpdate).
        // This bridges the Update/FixedUpdate timing gap so no input is ever dropped.
        private bool  _jumpRequested;
        private bool  _dashRequested;
        private bool  _shootRequested;
        private float _pendingRotationInput;   // -1, 0, +1 — cleared after StartRotation()

        // ── Rotation ──────────────────────────────────────────────────────────────
        private bool  _isRotating;
        private float _rotationTimer;
        private float _currentYAngle;
        private float _targetYAngle;
        
        // ── Player State Machine ──────────────────────────────────────────────────────────────
        public PlayerStateMachine StateMachine; 
        // ─────────────────────────────────────────────────────────────────────────
        private void Start()
        {
            motor.CharacterController = this;
            StateMachine = new PlayerStateMachine(this, CharacterState.Grounded);
            _arrowLauncher = GetComponent<ArrowLauncher>();
        }

        /// <summary>
        /// Called every Update by PlayerInputHandler.
        /// Latches one-shot inputs (jump, rotation) so they survive until the
        /// next FixedUpdate even if Update runs multiple times between physics steps.
        /// </summary>
        public void SetInputs(ref PlayerInputs inputs)
        {
            _inputs = inputs;

            if (inputs.JumpPressed)  _jumpRequested        = true;
            if (inputs.DashPressed)  _dashRequested        = true;
            if (inputs.ShootPressed) _shootRequested      = true;
            if (inputs.RotationInput != 0f) _pendingRotationInput = inputs.RotationInput;
        }

        // ── ICharacterController callbacks ────────────────────────────────────────

        public void BeforeCharacterUpdate(float deltaTime)
        {
            // ── ROTATION ──
            HandleRotationInput();

            // ── DASH ──
            if (_dashRequested && dashCooldownTimer <= 0f && CurrentState != CharacterState.Dashing)
            {
                dashCooldownTimer = dashCooldown;
                _dashRequested = false;
                StateMachine.TransitionToState(CharacterState.Dashing);
            }
            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - deltaTime);
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (!_isRotating) return;

            _rotationTimer += deltaTime / rotationDuration;
            _rotationTimer  = Mathf.Clamp01(_rotationTimer);

            float t    = Mathf.SmoothStep(0f, 1f, _rotationTimer);
            float newY = Mathf.LerpAngle(_currentYAngle, _targetYAngle, t);
            currentRotation = Quaternion.Euler(0f, newY, 0f);

            if (_rotationTimer < 1f) return;

            _isRotating    = false;
            _targetYAngle  = Mathf.Round(_targetYAngle / stepAngle) * stepAngle;
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (CurrentState)
            {
                case CharacterState.Grounded: HandleGroundedVelocity(ref currentVelocity, deltaTime); break;
                case CharacterState.Airborne: HandleAirborneVelocity(ref currentVelocity, deltaTime); break;
                case CharacterState.Dashing:  HandleDashVelocity(ref currentVelocity, deltaTime);     break;
                case CharacterState.Climbing: HandleClimbVelocity(ref currentVelocity, deltaTime);    break;
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            switch (IsGrounded)
            {
                // KCC has finished grounding detection — now is the safe moment to
                // switch states based on whether we're on the ground or not.
                case true when CurrentState == CharacterState.Airborne:
                    StateMachine.TransitionToState(CharacterState.Grounded);
                    break;

                case false when CurrentState == CharacterState.Grounded:
                    StateMachine.TransitionToState(CharacterState.Airborne);
                    break;
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {

            // Clear latched flags AFTER the motor has consumed them this frame.
            _jumpRequested  = false;
            _dashRequested  = false;
            if (_shootRequested)
            {
                if (_arrowLauncher != null)
                    _arrowLauncher.TryLaunch(motor.CharacterForward);
                else
                    Debug.LogError("ArrowLauncher component missing on " + gameObject.name, this);
                _shootRequested = false;
            }
        }

        // ── Velocity handlers ─────────────────────────────────────────────────────

        private void HandleGroundedVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Reorient current velocity to the slope normal so speed is preserved on ramps.
            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

            if (_jumpRequested)
            {
                motor.ForceUnground();  // tells KCC to stop snapping to ground this frame
                currentVelocity += (jumpUpSpeed * motor.CharacterUp)
                                   - Vector3.Project(currentVelocity, motor.CharacterUp);
                OnJumped?.Invoke();
                // State transition to Airborne happens in PostGroundingUpdate automatically.
                return;
            }

            Vector3 targetVelocity = ComputeMoveDirection() * maxMoveSpeed;

            // Exponential smoothing — frame-rate independent, same feel as Lerp but stable.
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity,
                1f - Mathf.Exp(-movementSharpness * deltaTime));
        }

        private void HandleAirborneVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Partial air control: player can steer but not instantly change direction.
            if (_inputs.MoveInput.sqrMagnitude > 0.01f)
            {
                Vector3 targetHorizontal = ComputeMoveDirection() * maxAirMoveSpeed;
                Vector3 velocityDiff     = Vector3.ProjectOnPlane(targetHorizontal - currentVelocity, gravity.normalized);
                currentVelocity += deltaTime * airAcceleration * velocityDiff;
            }

            currentVelocity += gravity * deltaTime;
        }

        
        private void HandleDashVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity  = dashDirection * dashSpeed;
            currentVelocity.y = 0f;   // keep it horizontal
            dashDurationTimer -= deltaTime;
            if (dashDurationTimer <= 0f)
                StateMachine.TransitionToState(IsGrounded
                    ? CharacterState.Grounded
                    : CharacterState.Airborne);
        }

        private void HandleClimbVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            var climbInput = _inputs.ClimbInput.y;
            var moveInput = _inputs.MoveInput.x;
            // TODO: implement jump during climbing state
            var jumpInput = _inputs.JumpPressed;

            float xDirection = transform.forward.x;
            float zDirection = transform.forward.z;

            if (zDirection > 0f || xDirection > 0f)
            {
                if (moveInput > 0.01f || jumpInput)
                {
                    StateMachine.TransitionToState(IsGrounded
                        ? CharacterState.Grounded
                        : CharacterState.Airborne);
                }
            }
            else if (zDirection < 0f || xDirection < 0f)
            {
                if (moveInput < -0.01f || jumpInput)
                {
                    StateMachine.TransitionToState(IsGrounded
                        ? CharacterState.Grounded
                        : CharacterState.Airborne);
                }
            }
                
            //transform.Rotate(Vector3.up, transform.rotation.y - );
            //motor.RotateCharacter();
            currentVelocity.y = climbInput;


        }

        // ── Shared helpers ────────────────────────────────────────────────────────

        public Vector3 ComputeMoveDirection()
        {
            if (_inputs.MoveInput.sqrMagnitude < 0.01f) return Vector3.zero;

            return (_inputs.CameraForward * _inputs.MoveInput.y
                    + _inputs.CameraRight  * _inputs.MoveInput.x).normalized;
        }

        private void HandleRotationInput()
        {
            if (_isRotating || _pendingRotationInput == 0f) return;

            Debug.Log(_pendingRotationInput);

            _targetYAngle += (stepAngle * _pendingRotationInput); // * _pendingRotationInput is used to rotate by 180° if _pendingRotationInput = 2 is passed

            _currentYAngle       = motor.TransientRotation.eulerAngles.y;
            _rotationTimer       = 0f;
            _isRotating          = true;
            _pendingRotationInput = 0f;   // consumed
        }

        public void SetPendingRotationInput(int pendingRotation)
        {
            _pendingRotationInput = pendingRotation;
        }

        // ── Unused required ICharacterController methods ──────────────────────────

        public bool IsColliderValidForCollisions(Collider coll) => true;
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    }
}