using System;
using System.Collections.Generic;
using Core;
using Sound;
using UnityEngine;

namespace Player
{
    public class PlayerSoundFXs : MonoBehaviour
    {
        private Dictionary<SurfaceType, AudioClip> footsteps = new();
        private Dictionary<SurfaceType, AudioClip> jumpLandings = new();

        [Header("Bow")]
        public AudioClip bowDraw;
        public AudioClip bowShoot;

        [SerializeField]
        public AudioClip[] climbSfx;

        private PlayerCharacterController _player;

        private void Start()
        {
            _player = GetComponent<PlayerCharacterController>();

            climbSfx = new[] {
                Resources.Load<AudioClip>("Sound/wooden_ladder_1"),
                Resources.Load<AudioClip>("Sound/wooden_ladder_2"),
                Resources.Load<AudioClip>("Sound/wooden_ladder_3"),
            };

            foreach (SurfaceType surface in Enum.GetValues(typeof(SurfaceType)))
            {
                string materialName = Enum.GetName(surface.GetType(), surface)?.ToLower();
                Debug.Log($"Sound/footstep_{materialName}");
                footsteps[surface] = Resources.Load<AudioClip>($"Sound/footstep_{materialName}");
                jumpLandings[surface] = Resources.Load<AudioClip>($"Sound/jump_land_{materialName}");
            }
        }

        private void OnFootStep()
        {
            var obj =  _player.CurrentGroundObject;
            var surface = obj.GetComponent<SurfaceTypeAttachment>();
            SurfaceType material = surface != null ? surface.material : SurfaceType.Stone;
            AudioManager.Instance.PlaySfx(footsteps[material], 1.5f);
        }

        private void OnJumpLand()
        {
            var obj =  _player.CurrentGroundObject;
            var surface = obj.GetComponent<SurfaceTypeAttachment>();
            SurfaceType material = surface != null ? surface.material : SurfaceType.Stone;
            AudioManager.Instance.PlaySfx(jumpLandings[material], 0.5f);
        }

        private void OnBowDraw() => AudioManager.Instance.PlaySfx(bowDraw, 0.7f);

        private void OnBowRelease() => AudioManager.Instance.PlaySfx(bowShoot, 0.7f);

        private void OnPlayerClimb() => AudioManager.Instance.PlayRandomSfx(climbSfx, 1.7f);
    }
}