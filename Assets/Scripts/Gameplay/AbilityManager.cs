using System.Collections.Generic;
using Core;
using HUD.Assist;
using Player;
using UnityEngine;

namespace Gameplay
{
    public class AbilityManager : MonoBehaviour
    {
        [Header("Ability GameObjects")]
        [SerializeField] public GameObject bow;
        [SerializeField] public GameObject quiver;
        [SerializeField] public GameObject theLastHeart;

        [SerializeField] public FeatureAssistData dashLocked;
        
        public ISet<AbilityType> UnlockedAbilities = new HashSet<AbilityType>();
        private HealthManager _healthManager;
        private BowController _bowController;
        private BowVisualsController _bowVisuals;

        private void Start()
        {
            // Access the scripts to disable if the ability isn't unlocked, needed
            // regardless of whether a save exists (e.g. a fresh New Game).
            var player = GameObject.FindGameObjectWithTag("Player");
            _healthManager = player.GetComponent<HealthManager>();
            _bowController = player.GetComponent<BowController>();
            _bowVisuals = player.GetComponent<BowVisualsController>();

            var save = SaveManager.Load();

            if (save?.unlockedAbilities != null)
                UnlockedAbilities = new HashSet<AbilityType>(save.unlockedAbilities);

            RefreshAbilityState();
        }

        public void UnlockAbility(AbilityType ability)
        {
            UnlockedAbilities.Add(ability);
            RefreshAbilityState();
        }

        public bool HasAbility(AbilityType ability) => UnlockedAbilities.Contains(ability);

        /// <summary>
        /// Applies the current ability set to the bow, instead of polling every frame in Update()
        /// for something that only ever changes right here or once at boot.
        /// </summary>
        private void RefreshAbilityState()
        {
            bool hasBow = HasAbility(AbilityType.Bow);

            _bowController.enabled = hasBow;
            _bowVisuals.enabled = hasBow;
            bow.SetActive(hasBow);
            quiver.SetActive(hasBow);

            var hasHealthUpgrade = HasAbility(AbilityType.Heart);
            if (hasHealthUpgrade)
            {
                HealthManager.UpgradeHealth();
                _healthManager.Heal(HealthManager.MaxHealth);
                theLastHeart.SetActive(true);
            }

            if (HasAbility(AbilityType.Dash))
            {
                TutorialAssistManager.I.DisableFeatureAssist(dashLocked);
            }

        }
    }
}