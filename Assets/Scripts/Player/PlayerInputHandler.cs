using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Reads the New Input System actions every Update, builds a PlayerInputs
    /// snapshot, and hands it to PlayerCharacterController.
    ///
    /// Responsibilities: input ONLY. No movement math lives here.
    /// Add new InputActionReferences here as you add new actions (dash, interact…).
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerCharacterController characterController;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference rotateLeftAction;
        [SerializeField] private InputActionReference rotateRightAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private InputActionReference shootAction;
        [SerializeField] private InputActionReference climbAction;
        // TODO improve input system so that it can be used with other objects (other than player)
        public InputActionReference interactAction;
    
        private Transform _cameraTransform;

        private void Start()
        {
            characterController = gameObject.GetComponent<PlayerCharacterController>();
            _cameraTransform = GameObject.Find("PlayerCamera").transform; 
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            rotateLeftAction.action.Enable();
            rotateRightAction.action.Enable();
            jumpAction.action.Enable();
            if (dashAction != null) dashAction.action.Enable();
            shootAction.action.Enable();
            climbAction.action.Enable();
            interactAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            rotateLeftAction.action.Disable();
            rotateRightAction.action.Disable();
            jumpAction.action.Disable();
            dashAction.action.Disable();
            shootAction.action.Disable();
            climbAction.action.Disable();
            interactAction.action.Disable();
        }

        private void Update()
        {
            // Flatten camera axes to the horizontal plane so vertical camera tilt
            // doesn't affect movement direction.
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight   = _cameraTransform.right;
            camForward.y = 0f;
            camRight.y   = 0f;
            camForward.Normalize();
            camRight.Normalize();

            MovementInputs moveInputs = new MovementInputs
            {
                MoveInput = moveAction.action.ReadValue<Vector2>(),
                CameraForward = camForward,
                CameraRight = camRight,
                ClimbInput = climbAction.action.ReadValue<Vector2>()
            };

            PlayerCommand commands = PlayerCommand.None;
            commands |= rotateLeftAction.action.triggered ? PlayerCommand.RotateCameraLeft : 0;
            commands |= rotateRightAction.action.triggered ? PlayerCommand.RotateCameraRight : 0;
            commands |= jumpAction.action.triggered ? PlayerCommand.Jump : 0;
            commands |= dashAction.action.triggered ? PlayerCommand.Dash : 0;
            commands |= shootAction.action.triggered ? PlayerCommand.Shoot : 0;
            commands |= interactAction.action.triggered ? PlayerCommand.Interact : 0;

            characterController.SetInputs(moveInputs, commands);
        }
    }
}
