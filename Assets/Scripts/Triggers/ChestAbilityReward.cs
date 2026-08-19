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
        [SerializeField] private float rewardSpawnDelay = 0.5f;
        [SerializeField] private float rewardRiseHeight = 0.8f;
        [SerializeField] private float animationDuration = 0.6f;
        [SerializeField] private float scaleMult = 2.5f;

        [Header("Chest Opening")]
        [SerializeField] private bool openAutomaticallyOnPlayerEnter = true;
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private GameObject openedVisual;

        [Header("Optional Animator")]
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private string openTriggerName = "OpenChest";

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
                _opened = true;
        }

        private void Update()
        {
            if (!_playerInside || _opened) return;

            if (openAutomaticallyOnPlayerEnter) return;

            if (CommandUtils.IsUp(_characterController.commands, PlayerCommand.Interact))
            {
                OpenChest();
                CommandUtils.Off(ref _characterController.commands, PlayerCommand.Interact);
            }
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

            if (chestAnimator)
                chestAnimator.SetTrigger(openTriggerName);

            StartCoroutine(SpawnRewardRoutine());
        }

        private IEnumerator SpawnRewardRoutine()
        {
            if (!rewardPrefab || !rewardSpawnPoint)
                yield break;

            if (CommandUtils.IsUp(_characterController.commands, PlayerCommand.Interact))
                yield return new WaitForSeconds(rewardSpawnDelay);

            Vector3 startPosition = rewardSpawnPoint.position;
            Vector3 endPosition = startPosition + Vector3.up * rewardRiseHeight;

            GameObject reward = Instantiate(
                rewardPrefab,
                startPosition,
                rewardSpawnPoint.rotation
            );
            Vector3 initialScale = reward.transform.localScale;

            AbilityRewardClaimHandler claimHandler = reward.GetComponent<AbilityRewardClaimHandler>();

            if (!claimHandler)
                claimHandler = reward.AddComponent<AbilityRewardClaimHandler>();

            claimHandler.Configure(rewardId, abilityToUnlock);

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / animationDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                reward.transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);
                reward.transform.localScale = Vector3.Lerp(initialScale, initialScale * scaleMult, smoothT*2);

                yield return null;
            }

            reward.transform.position = endPosition;
            // reward.transform.localScale = endScale;
        }
    }
}
