using UnityEngine;

namespace Player
{

    public class PlayerCameraController : MonoBehaviour
    {
        private static readonly Vector3[] Cardinals =
        {
            Vector3.left,
            Vector3.back,
            Vector3.right,
            Vector3.forward
        };

        [Header("Rotation")]
        [SerializeField] private float stepAngle = 90f;
        [SerializeField] private float rotationDuration = 0.3f;

        [Header("Distance")]
        [SerializeField] private float cameraDistance = 5f;

        private PlayerCharacterController _player;
        private int _currentDirection;

        private bool _isRotating;
        private float _rotationTimer;
        private float _currentYAngle;
        private float _targetYAngle;

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            _player = player.GetComponent<PlayerCharacterController>();
            transform.position = player.transform.position;

            var cameraInstance = transform.GetChild(0);
            cameraInstance.position = player.transform.position - cameraDistance * Vector3.forward;
        }

        /// <summary>
        /// Camera rotation player inputs
        /// </summary>
        private void FixedUpdate()
        {
            var pendingRotationInput = 0;

            if (CommandUtils.IsUp(_player.commands, PlayerCommand.RotateCameraLeft))
            {
                pendingRotationInput = -1;
            }
            else if (CommandUtils.IsUp(_player.commands, PlayerCommand.RotateCameraRight))
            {
                pendingRotationInput = 1;
            }

            if (_isRotating || pendingRotationInput == 0)
                return;

            _targetYAngle += stepAngle * pendingRotationInput;
            _targetYAngle %= 360;

            //Set new cardinal direction for camera
            var newRot = (_currentDirection + pendingRotationInput) % 4;
            _currentDirection = newRot < 0 ? newRot + 4 : newRot;

            _currentYAngle = transform.rotation.eulerAngles.y;
            Debug.Log($"target: {_targetYAngle} - current: {_currentYAngle}");

            _rotationTimer = 0f;
            _isRotating = true;

            CommandUtils.Off(ref _player.commands, PlayerCommand.RotateCameraLeft | PlayerCommand.RotateCameraRight);
        }

        /// <summary>
        /// Camera movement is in LateUpdate to allow it to happen after all others GameObject updates
        /// (e.g.: player movement)
        /// </summary>
        private void LateUpdate()
        {
            //link camera holder position to player
            transform.position = _player.gameObject.transform.position;

            if (!_isRotating)
            {
                return;
            }

            _rotationTimer += Time.deltaTime / rotationDuration;
            _rotationTimer = Mathf.Clamp01(_rotationTimer);

            float t = Mathf.SmoothStep(0f, 1f, _rotationTimer);
            float newY = Mathf.LerpAngle(_currentYAngle, _targetYAngle, t);
            transform.rotation = Quaternion.Euler(0, newY, 0);

            if (_player.CurrentState == _player.StateMachine.GroundedState)
                _player.SnapPlayerLocation(t);

            if (_rotationTimer >= 1f)
            {
                _isRotating = false;
            }
        }

        public float GetRotationY()
        {
            return Mathf.Round(_targetYAngle / stepAngle) * stepAngle;
        }

        public void SetRotationY(float yAngle)
        {
            _isRotating = false;
            _rotationTimer = 0f;
            _currentYAngle = yAngle;
            _targetYAngle = yAngle;

            transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
        }
    }
}