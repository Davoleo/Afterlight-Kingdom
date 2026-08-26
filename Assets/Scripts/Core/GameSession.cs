using System;
using UnityEngine;

namespace Core
{
    // Tracks the level the player is currently in and resolves which level
    // should be loaded when entering the Core scene.
    public static class GameSession
    {
        // Level a brand-new game starts at. Only ever read as a fallback in
        // ResolveLevelToLoad when there's no save to resume from - not the level the
        // player is currently in, use CurrentLevel for that.
        public const SceneNames NewGameStartLevel = SceneNames.Level1;

        public static SceneNames CurrentLevel { get; private set; } = SceneNames.Core;

        public static event Action<SceneNames> LevelChanged;

        public static void SetCurrentLevel(SceneNames level)
        {
            Debug.Log(CurrentLevel + " - " + level);

            if (CurrentLevel == level) return;

            CurrentLevel = level;
            LevelChanged?.Invoke(level);
        }

        public static string  ResolveLevelToLoad()
        {
            SaveData save = SaveManager.HasSave ? SaveManager.Load() : null;
            string savedLevel = save?.levelName;

            // Core is the boot/hub scene, never a real level - a save file that names it
            // (e.g. written while the active scene hadn't switched to the level yet) must
            // not be trusted, or CoreLoader would try to load Core as if it were a level.
            if (string.IsNullOrEmpty(savedLevel) || savedLevel == nameof(SceneNames.Core)) return NewGameStartLevel.ToString();

            return savedLevel;
        }
    }
}
