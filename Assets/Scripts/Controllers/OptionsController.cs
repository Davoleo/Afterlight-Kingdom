using Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class OptionsController : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private GameSettings settings;
        
        private bool _isInitializing;
        
        private void Start()
        {
            _isInitializing = true;
    
            settings.Load();
            volumeSlider.value = settings.masterVolume;
            fullscreenToggle.isOn = settings.fullscreen;
    
            _isInitializing = false;
        }

        public void OnAnyChange()
        {
            if (_isInitializing) return;
    
            settings.masterVolume = volumeSlider.value;
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
