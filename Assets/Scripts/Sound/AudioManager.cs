using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sound
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [SerializeField] private AudioSource globalSfxSource;
        [SerializeField] private AudioSource bgmSource;

        private static Dictionary<SceneNames, BGM> IndexedBgm;

        private Coroutine _bgmCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Sound.BGM.InitClips();
            IndexedBgm = new()
            {
                { SceneNames.Level1, Sound.BGM.Forest },
                { SceneNames.Level2, Sound.BGM.Village },
                { SceneNames.Level3, Sound.BGM.Castle }
            };
        }

        public void SetVolumes(float bgm, float sfx)
        {
            bgmSource.volume = bgm;
            globalSfxSource.volume = sfx;
        }

        public void PlaySfx(AudioClip clip, float volumeMult = 1f)
        {
            //Debug.Log("Playing SFX at volume: " + _volume + '*' + volumeMult);
            globalSfxSource.PlayOneShot(clip, volumeMult);
        }


        /// <summary>
        /// Plays a random sound effect from a pool with a given volume
        /// TODO: IMPLEMENT PITCH CONTROL
        /// </summary>
        /// <param name="pool">pool of SFXs to play the clip from</param>
        /// <param name="volumeMult">volume of playback</param>
        public void PlayRandomSfx(AudioClip[] pool, float volumeMult = 1f)
        {
            int randomIndex = Random.Range(0, pool.Length);
            globalSfxSource.PlayOneShot(pool[randomIndex], volumeMult);
        }

        /// <summary>
        /// Play BGM
        /// </summary>
        /// <param name="override">optional override scene BGM</param>
        public void PlayBGM(BGM @override = null) => _bgmCoroutine = StartCoroutine(BGM(@override));

        public IEnumerator BGM(BGM @override)
        {
            while (true)
            {
                if (@override is not null)
                {
                    bgmSource.clip = @override.UseOne();
                }
                else
                {
                    IndexedBgm.TryGetValue(GameSession.LevelToLoad, out var bgm);
                    //Scene with no music
                    if (bgm is null) yield break;

                    bgmSource.clip = bgm.UseOne();
                }

                bgmSource.Play();

                yield return new WaitForSeconds(60f);
            }
        }

        public void StopBGM()
        {
            StopCoroutine(_bgmCoroutine);
        }
    }
}