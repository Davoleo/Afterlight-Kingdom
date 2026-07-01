using System;
using System.Collections.Generic;
using Gameplay;

namespace Core
{
    [System.Serializable]
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
        
        //original player view rotation
        public float playerRotationX;
        public float playerRotationY;
        public float playerRotationZ;

        public List<string> collectedIds;

        public int coins;
        public int keys;
        public List<EnemySaveData> enemyStates = new List<EnemySaveData>();

        public List<AbilityType> unlockedAbilities;
    }
}