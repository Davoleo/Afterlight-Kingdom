using System;
using System.Collections.Generic;
using Gameplay;

namespace Core
{
    [Serializable]
    public class EnemySaveData
    {
        public string id;
        public bool isAlive;
    }

    [Serializable]
    public class SaveData
    {
        public float checkpointX;
        public float checkpointY;
        public float checkpointZ;
        public float cameraRotation;

        public List<string> collectedIds;

        public int coins;
        public int keys;
        public List<EnemySaveData> enemyStates = new();

        public List<AbilityType> unlockedAbilities;
    }
}