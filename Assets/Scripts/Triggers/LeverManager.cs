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

        private static int count;
        public FeatureAssistData[] interactSpeech;
        public AudioClip switchClip;

        private void Awake()
        {
            _leverAnimator = GetComponent<Animator>();
            var save = SaveManager.Load();
            if (save == null)
                return;
            foreach (var data in save.leverStates)
            {
                if (data.position == transform.position)
                {
                    SetState(data.flicked);
                }
            }
        }

        public void Pull()
        {
            flicked = !flicked;

            LeverStateChanged?.Invoke(flicked);
            AudioManager.Instance.PlaySfx(switchClip);
            var interact = interactSpeech[count];
            TutorialAssistManager.I.ShowAssist(interact);
            TutorialAssistManager.I.DisableFeatureAssist(interact);
            count = flicked ? Mathf.Min(count + 1, 3) : Mathf.Max(count - 1, 0);
            
            _leverAnimator.SetBool(FlickedHash, flicked);
        }

        private void SetState(bool state)
        {
            flicked = state;
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

            public static void InflateData(List<Persistence> leverData)
            {
                var levers = FindObjectsByType<LeverManager>(FindObjectsSortMode.None);
                for (var i = 0; i < levers.Length; i++)
                {
                    var lever = levers[i];
                    if (lever.transform.position == leverData[i].position)
                        lever.SetState(leverData[i].flicked);
                }
            }
        }
    }
}