using System.Collections.Generic;
using Core;
using Player;
using UnityEngine;

namespace Gameplay
{
    public class AbilityManager : MonoBehaviour
    {
        [Header("GameObjects to disable")] 
        [SerializeField] public GameObject bow;
        [SerializeField] public GameObject quiver;
        
        public ISet<AbilityType> UnlockedAbilities = new HashSet<AbilityType>();
        private GameObject _player;
        private BowController _bowController;
        private BowVisualsController _bowVisuals;

        private void Start()
        {
            // Access the scripts to disable if the ability isn't unlocked, needed
            // regardless of whether a save exists (e.g. a fresh New Game).
            _player = GameObject.FindGameObjectsWithTag("Player")[0];
            _bowController = _player.GetComponent<BowController>();
            _bowVisuals = _player.GetComponent<BowVisualsController>();

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

        // Applies the current ability set to the bow, instead of polling every frame in Update()
        // for something that only ever changes right here or once at boot.
        private void RefreshAbilityState()
        {
            bool hasBow = HasAbility(AbilityType.Bow);

            _bowController.enabled = hasBow;
            _bowVisuals.enabled = hasBow;
            bow.SetActive(hasBow);
            quiver.SetActive(hasBow);
        }
    }
}