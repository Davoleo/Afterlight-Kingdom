using System;
using HUD.Assist;
using Player;
using Sound;
using UnityEngine;

namespace Triggers
{
    public class LeverInteractionHandler : MonoBehaviour
    {
        private static readonly int FlickedHash = Animator.StringToHash("Flicked");

        private bool _playerInRange;
        private bool flicked;
        private static int _current;

        private PlayerCharacterController _player;
        private Animator _leverAnimator;

        public static event Action<bool> LeverStateChanged;

        public FeatureAssistData[] interactSpeech;
        public AudioClip switchClip;

        private void Start()
        {
            _leverAnimator = GetComponent<Animator>();
        }

        public void Pull()
        {
            flicked = !flicked;

            LeverStateChanged?.Invoke(flicked);
            AudioManager.Instance.PlaySfx(switchClip);
            TutorialAssistManager.I.ShowAssist(interactSpeech[_current++]);
            
            _leverAnimator.SetBool(FlickedHash, flicked);
        }
    }
}