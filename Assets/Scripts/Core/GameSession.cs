namespace Core
{
    // Tracks the level the player is currently in and resolves which level
    // should be loaded when entering the Core scene.
    public static class GameSession
    {
        public static string LevelToLoad = SceneNames.Level1;

        public static string ResolveLevelToLoad()
        {
            SaveData save = SaveManager.HasSave ? SaveManager.Load() : null;
            return !string.IsNullOrEmpty(save?.levelName) ? save.levelName : LevelToLoad;
        }
    }
}
