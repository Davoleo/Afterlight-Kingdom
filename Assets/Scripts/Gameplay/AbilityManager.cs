using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Gameplay
{
    public class AbilityManager : MonoBehaviour
    {
        public List<string> unlockedAbilities = new List<string>();

        private void Start()
        {
            SaveData save = SaveManager.Load();

            if (save == null || save.unlockedAbilities == null)
                return;

            unlockedAbilities = new List<string>(save.unlockedAbilities);
        }

        public void UnlockAbility(AbilityType ability)
        {
            string abilityId = ability.ToString();

            if (unlockedAbilities.Contains(abilityId))
                return;

            unlockedAbilities.Add(abilityId);

            Debug.Log("Ability unlocked: " + abilityId);
        }

        public bool HasAbility(AbilityType ability)
        {
            return unlockedAbilities.Contains(ability.ToString());
        }
    }
}