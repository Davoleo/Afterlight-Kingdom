using UnityEngine;

namespace Sound
{
    public class BGM
    {
        public static BGM Forest;
        public static BGM Village;
        public static BGM Castle;

        public static void InitClips()
        {
            Forest = new(Resources.Load<AudioClip>("Sound/BGM/forest1"));
            Village = new(Resources.Load<AudioClip>("Sound/bgm/village"));
            Castle = new(Resources.Load<AudioClip>("Sound/bgm/castle"));
        }


        private int _currentBGM;
        public AudioClip[] Clips { get; }

        private BGM(params AudioClip[] clips)
        {
            Clips = clips;
            _currentBGM = 0;
        }

        /// <summary>
        /// Extract one clip to use and bump the currentBgm to the next one
        /// </summary>
        /// <returns>extracted bgm clip</returns>
        public AudioClip UseOne()
        {
            var cur = _currentBGM;
            _currentBGM = (_currentBGM + 1) % Clips.Length;
            return Clips[cur];
        }
    }
}