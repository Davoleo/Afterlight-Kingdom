using System.Collections.Generic;
using UnityEngine;

namespace HUD.Assist
{
    public class TutorialAssistManager : MonoBehaviour
    {
        public static TutorialAssistManager Instance { get; private set; }

        [Tooltip("screen-space Fading prompt for technical info [anchored bottom-center]")] [SerializeField]
        private FadingPrompt technicalOverlay;

        [Tooltip("Player's Speech Bubble instance")] [SerializeField]
        private SpeechBubble playerSpeechBubble;

        private readonly HashSet<FeatureAssistData> disabledHints = new();

        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public bool IsAssistDisabled(FeatureAssistData feature) => disabledHints.Contains(feature);

        /// <summary>
        /// Marks a certain feature as completed disabling the assist tutorial trigger for it.
        /// </summary>
        /// <param name="feature">The completed feature</param>
        public void DisableFeatureAssist(FeatureAssistData feature) => disabledHints.Add(feature);

        public void ShowAssist(FeatureAssistData feature)
        {
            if (feature == null || IsAssistDisabled(feature)) return;

            technicalOverlay.Show(feature.TechicalPrompt, feature.DisplayDuration);
            playerSpeechBubble.Show(feature.NarrativePrompt, feature.DisplayDuration);
        }

    }
}