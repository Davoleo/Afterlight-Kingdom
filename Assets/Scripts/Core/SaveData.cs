using System;
using System.Collections.Generic;
using Gameplay;

namespace Core
{
    [Serializable]
    public class SaveData
    {
        public float checkpointX;
        public float checkpointY;
        public float checkpointZ;

        public List<string> collectedIds;

        public int coins;
        public int keys;

        public List<AbilityType> unlockedAbilities;
    }
}