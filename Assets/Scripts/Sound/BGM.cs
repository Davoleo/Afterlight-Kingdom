using UnityEngine;

namespace Sound
{
    public class BGM
    {
        public static BGM Forest;
        public static BGM Village;
        public static BGM Castle;

        private static AudioClip LoadClip(string path) => Resources.Load<AudioClip>(path);

        public static void InitClips()
        {
            Forest = new(LoadClip("Sound/bgm/forest1"), LoadClip("Sound/bgm/forest2"));
            Village = new(LoadClip("Sound/bgm/village1"), LoadClip("Sound/bgm/village2"));
            Castle = new(LoadClip("Sound/bgm/castle1"), LoadClip("Sound/bgm/castle2"));
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