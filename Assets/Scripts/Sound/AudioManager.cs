using System;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private void LazyInitVolume()
        {
            if (_volume < 0)
            {
                _volume = PlayerPrefs.GetFloat(GameSettings.MasterVolume);
            }
        }

        public void PlaySfx(AudioClip clip, float volumeMult = 1f)
        {
            LazyInitVolume();

            //Debug.Log("Playing SFX at volume: " + _volume + '*' + volumeMult);
            globalSfxSource.PlayOneShot(clip, _volume * volumeMult);
        }


        /// <summary>
        /// Plays a random sound effect from a pool with a given volume
        /// TODO: IMPLEMENT PITCH CONTROL
        /// </summary>
        /// <param name="pool">pool of SFXs to play the clip from</param>
        /// <param name="volumeMult">volume of playback</param>
        public void PlayRandomSfx(AudioClip[] pool, float volumeMult = 1f)
        {
            LazyInitVolume();

            int randomIndex = Random.Range(0, pool.Length);
            globalSfxSource.PlayOneShot(pool[randomIndex], _volume * volumeMult);
        }
    }
}