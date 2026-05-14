using KinematicCharacterController;
using Player;
using Player.State;
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

        [Header("Jump")]
        [SerializeField] public float jumpUpSpeed = 5f;

        [Header("Dash")]
        [SerializeField] private float dashCooldown = 2f;   // seconds
        public float   dashCooldownTimer;

        [Header("Rotation")]
        [SerializeField] private float stepAngle        = 90f;
        [SerializeField] private float rotationDuration = 0.3f;

        // ── Public state (consumed by PlayerAnimationController) ─────────────────
        public PlayerState CurrentState => StateMachine.CurrentState;
        public bool  IsGrounded    => motor.GroundingStatus.IsStableOnGround;
        public float ForwardSpeed  => Vector3.Dot(motor.Velocity, motor.CharacterForward);
        public float VerticalSpeed => Vector3.Dot(motor.Velocity, motor.CharacterUp);

        // ── Input cache ───────────────────────────────────────────────────────────
        public MovementInputs MoveInputs;
        // Latched flags: consumed in callbacks (FixedUpdate).
        // This bridges the Update/FixedUpdate timing gap so no input is ever dropped.
        public PlayerCommand commands;

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
            _arrowLauncher = GetComponent<ArrowLauncher>();
        }

        private void Awake()
        {
            StateMachine = new PlayerStateMachine(this);
            motor.CharacterController = this;
        }

        /// <summary>
        /// Called every Update by PlayerInputHandler.
        /// Latches one-shot inputs (jump, rotation) so they survive until the
        /// next FixedUpdate even if Update runs multiple times between physics steps.
        /// </summary>
        public void SetInputs(MovementInputs inputs, PlayerCommand pcommands)
        {
            MoveInputs = inputs;
            commands |= pcommands;
        }

        // ── ICharacterController callbacks ────────────────────────────────────────

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) => CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);

        public void BeforeCharacterUpdate(float deltaTime)
        {
            // ── ROTATION ──
            HandleRotationInput();

            // ── DASH ──
            if (CommandUtils.IsUp(commands, PlayerCommand.Dash) && dashCooldownTimer <= 0f && CurrentState != StateMachine.DashingState)
            {
                dashCooldownTimer = dashCooldown;
                CommandUtils.Off(ref commands, PlayerCommand.Dash);
                StateMachine.TransitionToState(StateMachine.DashingState);
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

        public void PostGroundingUpdate(float deltaTime)
        {
            switch (IsGrounded)
            {
                // KCC has finished grounding detection — now is the safe moment to
                // switch states based on whether we're on the ground or not.
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
            // Clear latched flags AFTER the motor has consumed them this frame.
            CommandUtils.Off(ref commands, PlayerCommand.Jump | PlayerCommand.Dash);
            if (CommandUtils.IsUp( commands, PlayerCommand.Shoot))
            {
                if (_arrowLauncher != null)
                    _arrowLauncher.TryLaunch(motor.CharacterForward);
                else
                    Debug.LogError("ArrowLauncher component missing on " + gameObject.name, this);
                CommandUtils.Off(ref commands, PlayerCommand.Shoot);
            }
        }
        
        // ── Shared helpers ────────────────────────────────────────────────────────

        public Vector3 ComputeMoveDirection()
        {
            if (MoveInputs.MoveInput.sqrMagnitude < 0.01f) return Vector3.zero;

            return (MoveInputs.CameraForward * MoveInputs.MoveInput.y
                    + MoveInputs.CameraRight  * MoveInputs.MoveInput.x).normalized;
        }

        private void HandleRotationInput()
        {
            var pendingRotationInput = 0F;
            if (CommandUtils.IsUp(commands, PlayerCommand.RotateCameraLeft))        pendingRotationInput = -1F;
            else if (CommandUtils.IsUp(commands, PlayerCommand.RotateCameraRight))   pendingRotationInput = 1F;

            if (_isRotating || pendingRotationInput == 0f) return;

            _targetYAngle += (stepAngle * pendingRotationInput); // * _pendingRotationInput is used to rotate by 180° if _pendingRotationInput = 2 is passed

            _currentYAngle       = motor.TransientRotation.eulerAngles.y;
            _rotationTimer       = 0f;
            _isRotating          = true;
            CommandUtils.Off(ref commands, PlayerCommand.RotateCameraLeft | PlayerCommand.RotateCameraRight);
        }

        // ── Unused required ICharacterController methods ──────────────────────────

        public bool IsColliderValidForCollisions(Collider coll) => true;
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    }
}