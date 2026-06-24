using Gameplay;
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

        [Header("UI")]
        [SerializeField] private GameObject promptObject;

        private bool _playerInside;
        private bool _claimed;

        private AbilityManager _abilityManager;
        private CollectiblesManager _collectiblesManager;

        private void Awake()
        {
            Collider rewardCollider = GetComponent<Collider>();
            rewardCollider.isTrigger = true;

            if (promptObject != null)
                promptObject.SetActive(false);
        }

        private void Start()
        {
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");

            if (gameManager != null)
                _collectiblesManager = gameManager.GetComponent<CollectiblesManager>();

            if (_collectiblesManager != null && _collectiblesManager.IsCollected(rewardId))
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (rotate)
            {
                transform.Rotate(
                    Vector3.up,
                    rotationSpeed * Time.deltaTime,
                    Space.World
                );
            }

            if (!_playerInside || _claimed)
                return;

            if (Keyboard.current != null &&
                Keyboard.current.fKey.wasPressedThisFrame)
            {
                ClaimReward();
            }
        }

        public void Configure(string id, AbilityType ability)
        {
            rewardId = id;
            abilityToUnlock = ability;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Reward trigger entered by: " + other.gameObject.name);

            if (!other.CompareTag("Player"))
                return;

            _playerInside = true;

            _abilityManager = other.GetComponentInParent<AbilityManager>();

            if (promptObject != null)
                promptObject.SetActive(true);

            Debug.Log("Press F to claim: " + abilityToUnlock);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _playerInside = false;

            if (promptObject != null)
                promptObject.SetActive(false);
        }

        private void ClaimReward()
        {
            if (_claimed)
                return;

            if (_abilityManager == null)
            {
                Debug.LogError(
                    "AbilityManager missing on Player.",
                    this
                );
                return;
            }

            _claimed = true;

            _abilityManager.UnlockAbility(abilityToUnlock);

            if (_collectiblesManager != null)
                _collectiblesManager.RegisterCollectedId(rewardId);

            if (promptObject != null)
                promptObject.SetActive(false);

            Debug.Log("Claimed reward: " + abilityToUnlock);

            gameObject.SetActive(false);
        }
    }
}