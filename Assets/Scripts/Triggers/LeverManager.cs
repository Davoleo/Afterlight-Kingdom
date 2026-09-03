using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using HUD.Assist;
using Player;
using Sound;
using UnityEngine;

namespace Triggers
{
    public class LeverManager : MonoBehaviour
    {
        private static readonly int FlickedHash = Animator.StringToHash("Flicked");

        private bool _playerInRange;
        private bool flicked;

        private PlayerCharacterController _player;
        private Animator _leverAnimator;

        public static event Action<bool> LeverStateChanged;

        public FeatureAssistData[] interactSpeech;
        public AudioClip switchClip;

        private void Awake()
        {
            _leverAnimator = GetComponent<Animator>();
            var save = SaveManager.Load();
            foreach (var data in save.leverStates.Where(data => data.position == transform.position))
            {
                flicked = data.flicked;
                _leverAnimator.SetBool(FlickedHash, flicked);
            }
        }

        public void Pull()
        {
            flicked = !flicked;

            LeverStateChanged?.Invoke(flicked);
            AudioManager.Instance.PlaySfx(switchClip);
            var first = interactSpeech.First(hint => !TutorialAssistManager.I.IsAssistDisabled(hint));
            TutorialAssistManager.I.ShowAssist(first);
            TutorialAssistManager.I.DisableFeatureAssist(first);

            
            _leverAnimator.SetBool(FlickedHash, flicked);
        }

        [Serializable]
        public class Persistence
        {
            private static readonly List<Persistence> LeverStates = new();

            public bool flicked;
            public Vector3 position;

            private Persistence(bool flicked, Vector3 position)
            {
                this.flicked = flicked;
                this.position = position;
            }

            public static List<Persistence> SqueezeIntoData()
            {
                LeverStates.Clear();

                var levers = FindObjectsByType<LeverManager>(FindObjectsSortMode.None);

                foreach (var lever in levers) LeverStates.Add(new Persistence(lever.flicked, lever.transform.position));

                return LeverStates;
            }
        }
    }
}