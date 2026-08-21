using System;
using Sound;
using UnityEngine;

namespace Player
{
    public class PlayerSoundFXs : MonoBehaviour
    {
        [Header("Main")]
        public AudioClip footstep;
        public AudioClip jumpLand;
        [Header("Bow")]
        public AudioClip bowDraw;
        public AudioClip bowShoot;

        [SerializeField]
        public AudioClip[] climbSfx;

        private void Start()
        {
            climbSfx = new[] {
                Resources.Load<AudioClip>("Sound/wooden_ladder_1"),
                Resources.Load<AudioClip>("Sound/wooden_ladder_2"),
                Resources.Load<AudioClip>("Sound/wooden_ladder_3"),
            };
        }

        private void OnFootStep(string surface)
        {
            AudioManager.Instance.PlaySfx(footstep, 1.5f);
        }

        private void OnJumpLand(string surface)
        {
            AudioManager.Instance.PlaySfx(jumpLand, 0.5f);
        }

        private void OnBowDraw()
        {
            AudioManager.Instance.PlaySfx(bowDraw, 0.7f);
        }

        private void OnBowRelease()
        {
            AudioManager.Instance.PlaySfx(bowShoot, 0.7f);
        }

        private void OnPlayerClimb()
        {
            AudioManager.Instance.PlayRandomSfx(climbSfx, 1.7f);
        }
    }
}