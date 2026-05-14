using Controllers;
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
            characterController.StateMachine.GroundedState.OnJumped += TriggerJump;
        }

        private void OnDisable()
        {
            characterController.StateMachine.GroundedState.OnJumped -= TriggerJump;
        }

        private void TriggerJump()
        {
            _animator.SetTrigger(JumpHash);
        }

        private void Update()
        {
            bool isGrounded = characterController.IsGrounded;
            _animator.SetFloat(SpeedHash,         characterController.ForwardSpeed);
            _animator.SetFloat(VerticalSpeedHash, characterController.VerticalSpeed);
            _animator.SetBool(GroundedHash,       isGrounded);
            _animator.SetBool(AirborneHash,       !isGrounded);
            _animator.SetBool(DashHash,           characterController.StateMachine.CurrentState == characterController.StateMachine.DashingState);
            _animator.SetBool(ClimbHash,          characterController.StateMachine.CurrentState == characterController.StateMachine.ClimbingState);
        }
    }
}