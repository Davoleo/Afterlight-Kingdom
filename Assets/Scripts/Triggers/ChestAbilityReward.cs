using System.Collections;
using Gameplay;
using Player;
using UnityEngine;

namespace Triggers
{
    public class ChestAbilityReward : MonoBehaviour
    {
        [Header("Reward")]
        [SerializeField] private string rewardId = "Level2_Bow";
        [SerializeField] private AbilityType abilityToUnlock = AbilityType.Bow;
        [SerializeField] private GameObject rewardPrefab;
        [SerializeField] private Transform rewardSpawnPoint;

        [Header("Reward Animation")]
        [SerializeField] private float rewardRiseHeight = 0.8f;
        [SerializeField] private float rewardRiseDuration = 0.6f;

        [Header("Chest Opening")]
        [SerializeField] private bool openAutomaticallyOnPlayerEnter = true;
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private GameObject openedVisual;

        [Header("Optional Animator")]
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private string openTriggerName = "Open";

        private bool _opened;
        private bool _playerInside;

        private CollectiblesManager _collectiblesManager;
        private PlayerCharacterController _characterController;

        private void Awake()
        {
            if (!chestAnimator) chestAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            var gameManager = GameObject.FindGameObjectWithTag("GameManager");

            _characterController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
            _collectiblesManager = gameManager.GetComponent<CollectiblesManager>();

            if (_collectiblesManager.IsCollected(rewardId))
            {
                _opened = true;
                SetChestVisualOpen(true);
                return;
            }

            SetChestVisualOpen(false);
        }

        private void Update()
        {
            if (!_playerInside || _opened) return;

            if (openAutomaticallyOnPlayerEnter) return;

            if (CommandUtils.IsUp(_characterController.triggers, PlayerTrigger.Interact))
                OpenChest();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _playerInside = true;

            if (openAutomaticallyOnPlayerEnter)
                OpenChest();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _playerInside = false;
        }

        private void OpenChest()
        {
            if (_opened)
                return;

            _opened = true;

            SetChestVisualOpen(true);

            if (chestAnimator && !string.IsNullOrWhiteSpace(openTriggerName))
                chestAnimator.SetTrigger(openTriggerName);

            StartCoroutine(SpawnRewardRoutine());
        }

        private IEnumerator SpawnRewardRoutine()
        {
            if (!rewardPrefab || !rewardSpawnPoint)
                yield break;

            Vector3 startPosition = rewardSpawnPoint.position;
            Vector3 endPosition = startPosition + Vector3.up * rewardRiseHeight;

            GameObject reward = Instantiate(
                rewardPrefab,
                startPosition,
                rewardSpawnPoint.rotation
            );

            AbilityRewardClaimHandler claimHandler = reward.GetComponent<AbilityRewardClaimHandler>();

            if (!claimHandler)
                claimHandler = reward.AddComponent<AbilityRewardClaimHandler>();

            claimHandler.Configure(rewardId, abilityToUnlock);

            float elapsed = 0f;

            while (elapsed < rewardRiseDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / rewardRiseDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                reward.transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);

                yield return null;
            }

            reward.transform.position = endPosition;
        }

        private void SetChestVisualOpen(bool isOpen)
        {
            if (closedVisual)
                closedVisual.SetActive(!isOpen);

            if (openedVisual)
                openedVisual.SetActive(isOpen);
        }
    }
}
