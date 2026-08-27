using Sound;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
    public class GameSettings : ScriptableObject
    {
        private const string BgmVolumeId = "BGMVolume";
        private const string SfxVolumeId = "SFXVolume";
        private const string FullScreenId = "FullScreen";

        [Range(0f, 1f)] public float bgmVolume = 0.4f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        public bool fullscreen = true;

        public void Apply()
        {
            AudioManager.Instance?.SetVolumes(bgmVolume, sfxVolume);
            Screen.fullScreen = fullscreen;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(BgmVolumeId, bgmVolume);
            PlayerPrefs.SetFloat(SfxVolumeId, sfxVolume);
            PlayerPrefs.SetInt(FullScreenId, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public void Load()
        {
            bgmVolume = PlayerPrefs.GetFloat(BgmVolumeId, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeId, 1f);
            fullscreen = PlayerPrefs.GetInt(FullScreenId, 1) == 1;
        }
    }
}