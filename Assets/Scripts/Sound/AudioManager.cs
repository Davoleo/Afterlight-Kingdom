using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sound
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = FindAnyObjectByType<AudioManager>();
                    if (_instance is null)
                    {
                        var prefab = Resources.Load<AudioManager>("AudioSystems");
                        _instance = prefab is not null ? Instantiate(prefab) : new GameObject("AudioSystems").AddComponent<AudioManager>();
                    }
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        [SerializeField] private AudioSource globalSfxSource;
        [SerializeField] private AudioSource bgmSource;

        private static Dictionary<SceneNames, BGM> IndexedBgm;

        private Coroutine _bgmCoroutine;

        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Sound.BGM.InitClips();
            IndexedBgm = new()
            {
                { SceneNames.Level1, Sound.BGM.Forest },
                { SceneNames.Level2, Sound.BGM.Village },
                { SceneNames.Level3, Sound.BGM.Castle }
            };
            PlayBGM(GameSession.CurrentLevel);

            GameSession.LevelChanged += newLevel =>
            {
                if (_bgmCoroutine is not null) StopBGM();

                PlayBGM(newLevel);
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

        /// <param name="scene">scene from which the appropriate bgm should be played</param>
        public void PlayBGM(SceneNames scene)
        {
            IndexedBgm.TryGetValue(scene, out var bgm);
            if (bgm is null) return;

            _bgmCoroutine = StartCoroutine(BGM(bgm));
        }

        public IEnumerator BGM(BGM bgm)
        {
            while (true)
            {
                bgmSource.clip = bgm.UseOne();
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