using System.Collections.Generic;
using System.IO;
using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            //var playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
                    
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == "Core")
            {
                // Should never happen - it means whatever called Save() ran before CoreLoader
                // finished switching the active scene to the level, or after it somehow reverted.
                // Logged (with stack trace) so the actual caller can be identified if this fires again.
                Debug.LogError($"SaveManager.Save: active scene is 'Core' at save time (caller: {gm.name}). " +
                                "This would corrupt the save's levelName - investigate before this write lands on disk.");
            }

            var data = new SaveData
            {
                levelName = activeSceneName,

                checkpointX = cpManager.LastCheckPoint.Position.x,
                checkpointY = cpManager.LastCheckPoint.Position.y,
                checkpointZ = cpManager.LastCheckPoint.Position.z,
                cameraRotation = cpManager.LastCheckPoint.Rotation,

                collectedIds = collManager.collectedIds ?? new List<string>(),
                coins = collManager.coins,
                keys = collManager.keys,

                unlockedAbilities = new List<AbilityType>(abilityManager.UnlockedAbilities ?? new HashSet<AbilityType>()),

                enemyStates = EnemySaveManager.GetEnemyStates()
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }
    }
}