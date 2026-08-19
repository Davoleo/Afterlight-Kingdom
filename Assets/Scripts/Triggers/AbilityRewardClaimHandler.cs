using Core;
using Gameplay;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class AbilityRewardClaimHandler : MonoBehaviour
    {
        [Header("Reward")]
        [SerializeField] private string rewardId;
        [SerializeField] private AbilityType abilityToUnlock = AbilityType.Bow;

        [Header("Rotation")]
        [SerializeField] private bool rotate = true;
        [SerializeField] private float rotationSpeed = 120f;

        private TextMeshProUGUI _promptText;
        private InputAction _interactAction;

        private PlayerCharacterController _characterController;
        private AbilityManager _abilityManager;
        private CollectiblesManager _collectiblesManager;
        private bool _playerInside;
        private bool _isGamepadActive;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            _promptText = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
            _promptText?.gameObject.SetActive(false);
        }

        private void Start()
        {
            var gameManager = GameObject.FindGameObjectWithTag("GameManager");
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            _characterController = playerObject.GetComponent<PlayerCharacterController>();

            if (gameManager == null)
            {
                Debug.LogError("AbilityRewardClaimHandler: GameManager not found.", this);
                enabled = false;
                return;
            }

            _collectiblesManager = gameManager.GetComponent<CollectiblesManager>();
            _abilityManager = gameManager.GetComponent<AbilityManager>();

            var inputHandler = playerObject.GetComponent<PlayerInputHandler>();
            _interactAction = inputHandler.interactAction.action;

            _isGamepadActive = InputUtils.IsGamepadActive();
            RefreshPromptText();

            if (_collectiblesManager.IsCollected(rewardId))
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (rotate)
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            bool gamepadActive = InputUtils.IsGamepadActive();
            if (gamepadActive != _isGamepadActive)
            {
                _isGamepadActive = gamepadActive;
                RefreshPromptText();
            }

            if (_playerInside && CommandUtils.IsUp(_characterController.triggers, PlayerTrigger.Interact))
                ClaimReward();
        }

        public void Configure(string id, AbilityType ability)
        {
            rewardId = id;
            abilityToUnlock = ability;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInside = true;
            _promptText?.gameObject.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInside = false;
            _promptText?.gameObject.SetActive(false);
        }

        private void ClaimReward()
        {
            enabled = false;

            _abilityManager.UnlockAbility(abilityToUnlock);
            _collectiblesManager.RegisterCollectedId(rewardId);
            _promptText?.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void RefreshPromptText()
        {
            string group = _isGamepadActive ? "Gamepad" : "Keyboard&Mouse";
            string keyLabel = _interactAction.GetBindingDisplayString(group: group);
            if (string.IsNullOrEmpty(keyLabel))
                keyLabel = _interactAction.GetBindingDisplayString();

            _promptText.text = $"[{keyLabel}] Collect {abilityToUnlock}";
        }

    }
}
