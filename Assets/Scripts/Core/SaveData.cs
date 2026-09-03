using System;
using System.Collections.Generic;
using Gameplay;
using Triggers;

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
        public string levelName;

        public float checkpointX;
        public float checkpointY;
        public float checkpointZ;
        public float cameraRotation;

        public List<string> collectedIds;
        public List<string> openedDoorIds;

        public int coins;
        public int keys;
        public List<EnemySaveData> enemyStates = new();

        public List<AbilityType> unlockedAbilities;

        public List<LeverManager.Persistence> leverStates;

        public List<string> leverState;
        public List<string> disabledHints;
        public List<string> seenHints;
    }
}