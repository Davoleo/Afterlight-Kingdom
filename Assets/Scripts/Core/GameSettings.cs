using System;
using System.IO;
using Player;
using Sound;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class GameSettings
    {
        private static string SettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");
        public static GameSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                Instance = new GameSettings();
                return Instance;
            }

            try
            {
                Instance = JsonUtility.FromJson<GameSettings>(File.ReadAllText(SettingsPath));
                //file read returned null -> reset settings;
                Instance ??= new GameSettings();
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to load settings.json: {e}");
                Instance = new GameSettings();
            }

            return Instance;
        }

        public static void Save()
        {
            if (Instance == null)
                return;

            try
            {
                File.WriteAllText(SettingsPath, JsonUtility.ToJson(Instance, true));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save settings.json: {e}");
            }
        }

        public static GameSettings Instance { get; private set; }

        public float bgmVolume = 0.4f;
        public float sfxVolume = 0.8f;
        public bool fullscreen = true;
        public bool invertRotation;

        public void Apply()
        {
            AudioManager.Instance?.SetVolumes(bgmVolume, sfxVolume);
            Screen.fullScreen = fullscreen;
            PlayerInputHandler.InvertCameraControls = invertRotation;
        }
    }
}