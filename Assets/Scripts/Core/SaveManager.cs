using System.Collections.Generic;
using System.IO;
using Gameplay;
using UnityEngine;
using Controllers;

namespace Core
{ 
    public static class SaveManager
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

        public static bool HasSave => File.Exists(SavePath);
        
        public static SaveData Load()
        {
            return !HasSave ? null : JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }

        public static void Delete()
        {
            if (HasSave) File.Delete(SavePath);
        }

        public static void Save(GameObject gm)
        {
            var cpManager = gm.GetComponent<CheckpointManager>();
            var collManager = gm.GetComponent<CollectiblesManager>();
            var abilityManager = gm.GetComponent<AbilityManager>();
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 playerRotation = player.transform.eulerAngles;
            var playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
            var data = new SaveData
            {
                checkpointX = cpManager.lastCheckPoint.x,
                checkpointY = cpManager.lastCheckPoint.y,
                checkpointZ = cpManager.lastCheckPoint.z,

                playerRotationX = playerRotation.x,
                playerRotationY = playerController.GetSavedRotationY(),
                playerRotationZ = playerRotation.z,




                collectedIds = collManager.collectedIds ?? new List<string>(),
                coins = collManager.coins,
                keys = collManager.keys,

                unlockedAbilities = new List<AbilityType>(abilityManager.UnlockedAbilities ?? new HashSet<AbilityType>())
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }
    }
}