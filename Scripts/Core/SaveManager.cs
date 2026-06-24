using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core
{
    public static class SaveManager
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

        public static bool HasSave => File.Exists(SavePath);

        public static void Save(Vector3 checkpoint, List<string> collectedIds, int coins, int keys)
        {
            var data = new SaveData
            {
                checkpointX = checkpoint.x,
                checkpointY = checkpoint.y,
                checkpointZ = checkpoint.z,
                collectedIds = collectedIds,
                coins = coins,
                keys = keys
            };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }

        public static SaveData Load()
        {
            return !HasSave ? null : JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }

        public static void Delete()
        {
            if (HasSave) File.Delete(SavePath);
        }
    }
}
