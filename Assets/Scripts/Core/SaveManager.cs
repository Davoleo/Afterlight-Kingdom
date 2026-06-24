using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Core
{
    public static class SaveManager
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "save.json");

        public static bool HasSave => File.Exists(SavePath);

        public static void Save(
            Vector3 checkpoint,
            List<string> collectedIds,
            int coins,
            int keys,
            List<string> unlockedAbilities)
        {
            var data = new SaveData
            {
                checkpointX = checkpoint.x,
                checkpointY = checkpoint.y,
                checkpointZ = checkpoint.z,

                collectedIds = collectedIds ?? new List<string>(),
                coins = coins,
                keys = keys,

                unlockedAbilities = unlockedAbilities ?? new List<string>()
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }

        public static SaveData Load()
        {
            if (!HasSave)
                return null;

            return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }

        public static void Delete()
        {
            if (HasSave)
                File.Delete(SavePath);
        }
    }
}