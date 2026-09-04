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

        // Leve vive nel livello caricato, usate per derivare quante sono flicked senza
        // tenere un contatore separato che può disallinearsi dopo un ripristino da save.
        private static readonly List<LeverManager> ActiveLevers = new();

        private bool _playerInRange;
        private bool flicked;

        private PlayerCharacterController _player;
        private Animator _leverAnimator;

        public static event Action<bool> LeverStateChanged;

        public FeatureAssistData[] interactSpeech;
        public AudioClip switchClip;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearRegistry() => ActiveLevers.Clear();

        // Sempre calcolato, mai memorizzato: riflette qualunque cosa Pull()/SetState()
        // abbia effettivamente fatto a `flicked` su ogni leva registrata, quindi non può
        // mai disallinearsi da un ripristino da save (a differenza di un contatore statico
        // incrementato a mano).
        private static int FlickedCount => ActiveLevers.Count(l => l.flicked);

        private void Awake()
        {
            _leverAnimator = GetComponent<Animator>();
            ActiveLevers.Add(this);

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

        private void OnDestroy()
        {
            ActiveLevers.Remove(this);
        }

        public void Pull()
        {
            // Puzzle già risolto prima di questo pull: leve morte anche per il sistema di
            // hint, stesso criterio con cui Level2GateController ignora ulteriori toggle.
            bool alreadySolved = FlickedCount == ActiveLevers.Count;

            int hintIndex = Mathf.Clamp(FlickedCount, 0, interactSpeech.Length - 1);

            flicked = !flicked;

            LeverStateChanged?.Invoke(flicked);
            AudioManager.Instance.PlaySfx(switchClip);

            if (!alreadySolved)
            {
                var interact = interactSpeech[hintIndex];
                TutorialAssistManager.I.ShowAssist(interact);
                TutorialAssistManager.I.DisableFeatureAssist(interact);
            }

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