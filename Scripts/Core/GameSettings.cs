using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
    public class GameSettings : ScriptableObject
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        public bool fullscreen = true;

        public void Apply()
        {
            AudioListener.volume = masterVolume;
            Screen.fullScreen = fullscreen;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetInt("FullScreen", fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public void Load()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            fullscreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        }
    }
}