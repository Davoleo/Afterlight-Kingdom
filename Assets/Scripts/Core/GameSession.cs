namespace Core
{
    // Tracks the level the player is currently in and resolves which level
    // should be loaded when entering the Core scene.
    public static class GameSession
    {
        public static SceneNames LevelToLoad = SceneNames.Level1;

        public static string  ResolveLevelToLoad()
        {
            SaveData save = SaveManager.HasSave ? SaveManager.Load() : null;
            string savedLevel = save?.levelName;

            // Core is the boot/hub scene, never a real level - a save file that names it
            // (e.g. written while the active scene hadn't switched to the level yet) must
            // not be trusted, or CoreLoader would try to load Core as if it were a level.
            if (string.IsNullOrEmpty(savedLevel) || savedLevel == nameof(SceneNames.Core)) return LevelToLoad.ToString();

            return savedLevel;
        }
    }
}
