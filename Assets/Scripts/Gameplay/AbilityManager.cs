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
            var save = SaveManager.Load();

            if (save?.unlockedAbilities == null) return;

            UnlockedAbilities = new HashSet<AbilityType>(save.unlockedAbilities);
            // Access the script to disable if ability is not unlocked
            _player = GameObject.FindGameObjectsWithTag("Player")[0];
            _bowController = _player.GetComponent<BowController>();
            _bowVisuals = _player.GetComponent<BowVisualsController>();
        }

        private void Update()
        {
            if (!HasAbility(AbilityType.Bow))
            {
                _bowController.enabled = false;
                _bowVisuals.enabled = false;
                bow.SetActive(false);
                quiver.SetActive(false);
            }
            else
            {
                _bowController.enabled = true;
                _bowVisuals.enabled = true;
                bow.SetActive(true);
                quiver.SetActive(true);
            }
        }

        public void UnlockAbility(AbilityType ability) => UnlockedAbilities.Add(ability);

        public bool HasAbility(AbilityType ability) => UnlockedAbilities.Contains(ability);
    }
}