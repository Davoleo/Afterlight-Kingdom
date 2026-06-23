using System;
using System.Collections.Generic;

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
    }
}
