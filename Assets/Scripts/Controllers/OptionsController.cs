using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;

        [SerializeField] private TextMeshProUGUI sfxLabel;
        [SerializeField] private TextMeshProUGUI bgmLabel;

        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private GameSettings settings;
        
        private bool _isInitializing; // prevent Toggles from calling OnAnyChange()
        
        private void Start()
        {
            _isInitializing = true;
    
            settings.Load();

            Debug.Log("SFX" + settings.sfxVolume + " BGM" + settings.bgmVolume);

            var volSfx = settings.sfxVolume * 100;
            sfxVolumeSlider.value = volSfx;
            sfxLabel.text = $"SFX: {volSfx}%";
            var volBGM = settings.bgmVolume * 100;
            bgmVolumeSlider.value = volBGM;
            bgmLabel.text = $"BGM: {volBGM}%";

            fullscreenToggle.isOn = settings.fullscreen;
    
            _isInitializing = false;
        }

        public void OnAnyChange()
        {
            if (_isInitializing) return;

            bgmLabel.text = $"BGM: {bgmVolumeSlider.value}%";
            settings.bgmVolume = bgmVolumeSlider.value / 100f;
            sfxLabel.text = $"SFX: {sfxVolumeSlider.value}%";
            settings.sfxVolume = sfxVolumeSlider.value / 100f;

            settings.fullscreen = fullscreenToggle.isOn;
            settings.Apply();
        }
        
        public void SaveChanges()
        {
            settings.Apply();
            settings.Save();
        }
    }
}
