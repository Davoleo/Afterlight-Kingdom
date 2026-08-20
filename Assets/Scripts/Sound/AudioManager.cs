using System;
using Core;
using UnityEngine;

namespace Sound
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [SerializeField] private AudioSource globalSfxSource;
        private float _volume = -1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlaySfx(AudioClip clip, float volumeMult = 1f)
        {
            if (_volume < 0)
            {
                _volume = PlayerPrefs.GetFloat(GameSettings.MasterVolume);
            }

            Debug.Log("Playing SFX at volume: " + _volume + '*' + volumeMult);
            globalSfxSource.PlayOneShot(clip, _volume * volumeMult);
        }
    }
}