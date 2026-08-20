using System;
using Sound;
using UnityEngine;

namespace Player
{
    public class PlayerSoundFXs : MonoBehaviour
    {
        public AudioClip footstep;

        private void OnFootStep(string surface)
        {

            AudioManager.Instance.PlaySfx(footstep);

        }
    }
}