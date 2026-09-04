using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Unity.VisualScripting;
using UnityEngine;

namespace HUD.Assist
{
    public class TutorialAssistManager : MonoBehaviour
    {
        public static TutorialAssistManager I { get; private set; }

        [Tooltip("screen-space Fading prompt for technical info [anchored bottom-center]")] [SerializeField]
        private FadingPrompt technicalOverlay;

        [Tooltip("Player's Speech Bubble instance")] [SerializeField]
        private SpeechBubble playerSpeechBubble;

        private readonly HashSet<string> _seenHints = new();
        private readonly HashSet<string> _disabledHints = new();

        private void Awake()
        {
            // Unity-aware null check (not `is not null`): a scene reload leaves the static
            // Instance pointing at the destroyed GameManager from the previous session.
            // `is not null` would see that stale managed reference as live and destroy the
            // NEW GameManager; `!= null` runs Unity's lifecycle check and treats it as null.
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;

            var save = SaveManager.Load();
            if (save == null)
                return;
            _disabledHints.AddRange(save.disabledHints);
            _seenHints.AddRange(save.seenHints);
        }

        private void OnDestroy()
        {
            if (I == this)
                I = null;
        }

        public bool IsAssistDisabled(FeatureAssistData feature) => _disabledHints.Contains(feature.Id);

        /// <summary>
        /// Marks a certain feature as completed disabling the assist tutorial trigger for it.
        /// </summary>
        /// <param name="feature">The completed feature</param>
        public void DisableFeatureAssist(FeatureAssistData feature)
        {
            if (IsAssistDisabled(feature))
                return;

            _disabledHints.Add(feature.Id);
            if (feature.DismissMode == AssistDismissMode.OnAction)
            {
                technicalOverlay.Hide();
                playerSpeechBubble.Hide();
            }
        }

        public bool HasBeenSeen(FeatureAssistData feature) => _seenHints.Contains(feature.Id);

        public void ShowAssist(FeatureAssistData feature)
        {
            if (feature == null || IsAssistDisabled(feature)) return;
            Display(feature);
        }

        public void EnsureSeen(FeatureAssistData feature)
        {
            if (feature is null || HasBeenSeen(feature)) return;
            Display(feature);
        }

        private void Display(FeatureAssistData feature)
        {
            _seenHints.Add(feature.Id);

            if (feature.DismissMode == AssistDismissMode.Timer)
            {
                technicalOverlay.Show(feature.TechicalPrompt, feature.DisplayDuration);
                playerSpeechBubble.Show(feature.NarrativePrompt, feature.DisplayDuration);
            }
            else
            {
                technicalOverlay.ShowHeld(feature.TechicalPrompt);
                playerSpeechBubble.ShowHeld(feature.NarrativePrompt);
            }
        }

        public Tuple<List<string>, List<string>> SqueezeOutRawIds() => new(_disabledHints.ToList(), _seenHints.ToList());
    }
}