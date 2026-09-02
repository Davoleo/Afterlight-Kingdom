using System.Collections;
using Gameplay;
using Player;
using Sound;
using UnityEngine;
using UnityEngine.Serialization;

namespace Triggers
{
    public class ChestAbilityReward : MonoBehaviour
    {
        [Header("Reward")]
        [SerializeField] private AbilityType abilityToUnlock = AbilityType.Bow;
        [SerializeField] private GameObject rewardPrefab;
        [SerializeField] protected Transform rewardSpawnPoint;

        [Header("Reward Animation")]
        [SerializeField] protected float rewardSpawnDelay = 0.5f;
        [SerializeField] private float rewardRiseHeight = 0.8f;
        [SerializeField] private float animationDuration = 0.6f;
        [SerializeField] private float scaleMult = 2.5f;

        [Header("Chest Opening")]
        [SerializeField] private bool openAutomaticallyOnPlayerEnter = true;

        [Header("Animation")]
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private string openTriggerName = "OpenChest";
        [SerializeField] private ParticleSystem particles;

        [FormerlySerializedAs("chestOpenSfx")] [Header("SFX")] [SerializeField] private AudioClip sfx;

        private bool _playerInside;

        protected bool Opened;
        protected PlayerCharacterController CharacterController;
        protected CollectiblesManager Collectibles;

        private void Awake()
        {
            if (!chestAnimator) chestAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            var gameManager = GameObject.FindGameObjectWithTag("GameManager");
            var abilityManager = gameManager.GetComponent<AbilityManager>();
            Collectibles = gameManager.GetComponent<CollectiblesManager>();

            CharacterController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();

            if (abilityManager.HasAbility(abilityToUnlock))
                Opened = true;
        }

        protected virtual bool CanActivate() => true;

        protected virtual void PlayActivationEffect()
        {
            if (chestAnimator)
                chestAnimator.SetTrigger(openTriggerName);
        }

        public void OpenChest()
        {
            if (!CanActivate()) return;

            if (Opened) return;

            Opened = true;

            PlayActivationEffect();

            if (sfx) AudioManager.Instance.PlaySfx(sfx, volumeMult: 1.5f);

            StartCoroutine(SpawnRewardRoutine());
        }

        private IEnumerator SpawnRewardRoutine()
        {
            if (!rewardPrefab || !rewardSpawnPoint)
                yield break;

            if (CommandUtils.IsUp(CharacterController.triggers, PlayerTrigger.Interact))
                yield return new WaitForSeconds(rewardSpawnDelay);

            if (particles) particles?.Play();

            Vector3 startPosition = rewardSpawnPoint.position;
            Vector3 endPosition = startPosition + Vector3.up * rewardRiseHeight;

            GameObject reward = Instantiate(
                rewardPrefab,
                startPosition,
                rewardSpawnPoint.rotation
            );
            Vector3 initialScale = reward.transform.localScale;

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
