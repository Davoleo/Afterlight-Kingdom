using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Gameplay
{
    public class AbilityManager : MonoBehaviour
    {
        public ISet<AbilityType> UnlockedAbilities = new HashSet<AbilityType>();

        private void Start()
        {
            var save = SaveManager.Load();

            if (save?.unlockedAbilities == null) return;

            UnlockedAbilities = new HashSet<AbilityType>(save.unlockedAbilities);
        }

        public void UnlockAbility(AbilityType ability) => UnlockedAbilities.Add(ability);

        public bool HasAbility(AbilityType ability) => UnlockedAbilities.Contains(ability);
    }
}