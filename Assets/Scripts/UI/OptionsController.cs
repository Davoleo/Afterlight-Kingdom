using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;

        [SerializeField] private TextMeshProUGUI sfxLabel;
        [SerializeField] private TextMeshProUGUI bgmLabel;

        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle rotationInverter;
        
        private bool _isInitializing; // prevent Toggles from calling OnAnyChange()
        private GameSettings _settings;
        
        private void Start()
        {
            _isInitializing = true;

            _settings = GameSettings.Load();

            Debug.Log("SFX" + _settings.sfxVolume + " BGM" + _settings.bgmVolume);

            var volSfx = _settings.sfxVolume * 100;
            sfxVolumeSlider.value = volSfx;
            sfxLabel.text = $"SFX: {volSfx}%";
            var volBGM = _settings.bgmVolume * 100;
            bgmVolumeSlider.value = volBGM;
            bgmLabel.text = $"BGM: {volBGM}%";

            fullscreenToggle.isOn = _settings.fullscreen;
            rotationInverter.isOn = _settings.invertRotation;

            _settings.Apply();
            _isInitializing = false;
        }

        public void OnAnyChange()
        {
            if (_isInitializing) return;

            bgmLabel.text = $"BGM: {bgmVolumeSlider.value}%";
            _settings.bgmVolume = bgmVolumeSlider.value / 100f;
            sfxLabel.text = $"SFX: {sfxVolumeSlider.value}%";
            _settings.sfxVolume = sfxVolumeSlider.value / 100f;

            _settings.fullscreen = fullscreenToggle.isOn;
            _settings.invertRotation = rotationInverter.isOn;
            _settings.Apply();
        }
        
        public void SaveChanges() => GameSettings.Save();
    }
}
