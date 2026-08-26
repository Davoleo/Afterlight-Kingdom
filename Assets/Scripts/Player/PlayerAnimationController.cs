using Core;
using Player.State;
using UnityEngine;

namespace Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterController characterController;
        private Animator _animator;

        private static readonly int SpeedHash          = Animator.StringToHash("Speed");
        private static readonly int GroundedHash       = Animator.StringToHash("Grounded");
        private static readonly int AirborneHash       = Animator.StringToHash("Airborne");
        private static readonly int DashHash           = Animator.StringToHash("Dash");
        private static readonly int JumpHash           = Animator.StringToHash("Jump");
        private static readonly int VerticalSpeedHash  = Animator.StringToHash("VerticalSpeed");
        private static readonly int ClimbHash          = Animator.StringToHash("Climbing");

        private void Start()
        {
            _animator = gameObject.GetComponent<Animator>();
        }

        private void OnEnable()
        {
            PlayerState.OnJumped += TriggerJump;
            GameStateManager.Respawned += ResetAnimatorState;
        }

        private void OnDisable()
        {
            PlayerState.OnJumped -= TriggerJump;
            GameStateManager.Respawned -= ResetAnimatorState;
        }

        private void TriggerJump()
        {
            _animator.SetTrigger(JumpHash);
        }

        // Clears every trigger/bool and returns all layers to their default state, so the
        // player never respawns mid-way through whatever animation was playing on death.
        private void ResetAnimatorState()
        {
            _animator.Rebind();
            _animator.Update(0f);
        }


        private void Update()
        {
            bool isGrounded = characterController.IsGrounded;
            float fwdSpeed = characterController.ForwardSpeed;
            float vertSpeed = characterController.VerticalSpeed;
            // Prevent the character from slowing down the running animation if stable on a slope
            float currentSpeed = isGrounded ? Mathf.Abs(fwdSpeed) + Mathf.Abs(vertSpeed) : fwdSpeed;
            
            _animator.SetFloat(SpeedHash,         currentSpeed);
            _animator.SetFloat(VerticalSpeedHash, vertSpeed);
            _animator.SetBool(GroundedHash,       isGrounded);
            _animator.SetBool(AirborneHash,       !isGrounded);
            _animator.SetBool(DashHash,           characterController.StateMachine.CurrentState == characterController.StateMachine.DashingState);
            _animator.SetBool(ClimbHash,          characterController.StateMachine.CurrentState == characterController.StateMachine.ClimbingState);
        }
    }
}