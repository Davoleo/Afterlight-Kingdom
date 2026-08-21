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

        private void OnFootStep(string surface)
        {
            AudioManager.Instance.PlaySfx(footstep);
        }

        private void OnJumpLand(string surface)
        {
            AudioManager.Instance.PlaySfx(jumpLand, 0.3f);
        }

        private void OnBowDraw()
        {
            AudioManager.Instance.PlaySfx(bowDraw);
        }

        private void OnBowRelease()
        {
            AudioManager.Instance.PlaySfx(bowShoot);
        }
    }
}