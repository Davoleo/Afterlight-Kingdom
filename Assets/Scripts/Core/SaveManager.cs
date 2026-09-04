using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gameplay;
using HUD.Assist;
using Triggers;
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

        public static void RestoreEverything(GameObject gm, SaveData data, bool fromMainMenu)
        {
            // var cpManager = gm.GetComponent<CheckpointManager>();
            // cpManager.SetCheckpoint(new Vector3(data.checkpointX, data.checkpointY, data.checkpointZ), data.cameraRotation);

            gm.GetComponent<CollectiblesManager>().RestoreFromSave(data, refreshGameObjects: fromMainMenu);
            gm.GetComponent<DoorManager>().RestoreFromSave(data);
            var am = gm.GetComponent<AbilityManager>();
            am.UnlockedAbilities = data.unlockedAbilities.ToHashSet();
            gm.GetComponent<TutorialAssistManager>().RestoreFromSave(data);
            LeverManager.Persistence.InflateData(data.leverStates);

            //restore enemies
            EnemySaveManager.RestoreEnemyStates(data.enemyStates);
        }

        public static void Save(GameObject gm)
        {
            var cpManager = gm.GetComponent<CheckpointManager>();
            var collManager = gm.GetComponent<CollectiblesManager>();
            var doorManager = gm.GetComponent<DoorManager>();
            var abilityManager = gm.GetComponent<AbilityManager>();
            var tutorialAssistData = gm.GetComponent<TutorialAssistManager>().SqueezeOutRawIds();
            var leverData = LeverManager.Persistence.SqueezeIntoData();


            string activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == "Core")
            {
                // Should never happen - it means whatever called Save() ran before CoreLoader
                // finished switching the active scene to the level, or after it somehow reverted.
                // Logged (with stack trace) so the actual caller can be identified if this fires again.
                // Bail out instead of writing: a save with levelName == "Core" gets silently
                // discarded by GameSession.ResolveLevelToLoad anyway, so writing it here would
                // just overwrite a possibly good previous save with a corrupted one.
                Debug.LogError($"SaveManager.Save: active scene is 'Core' at save time (caller: {gm.name}). " +
                                "Refusing to write - this would corrupt the save's levelName.");
                return;
            }

            var previousSave = Load();
            var data = new SaveData
            {
                levelName = activeSceneName,

                checkpointX = cpManager.LastCheckPoint.Position.x,
                checkpointY = cpManager.LastCheckPoint.Position.y,
                checkpointZ = cpManager.LastCheckPoint.Position.z,
                cameraRotation = cpManager.LastCheckPoint.Rotation,

                collectedIds = collManager.CollectedIds ?? new List<string>(),
                openedDoorIds = doorManager.openedDoorIds ?? new List<string>(),
                coins = collManager.Coins,
                keys = collManager.Keys,

                unlockedAbilities = new List<AbilityType>(abilityManager.UnlockedAbilities ?? new HashSet<AbilityType>()),

                enemyStates = EnemySaveManager.MergeEnemyStates(previousSave?.enemyStates),

                leverStates = leverData,

                disabledHints = tutorialAssistData.Item1,
                seenHints = tutorialAssistData.Item2
            };

            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }
    }
}