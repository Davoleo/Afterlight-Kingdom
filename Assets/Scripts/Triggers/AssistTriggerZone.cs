using HUD.Assist;
using UnityEngine;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class AssistTriggerZone : MonoBehaviour
    {
        [SerializeField] private FeatureAssistData[] features;
        [SerializeField] [Tooltip("Ensure the hint is displayed the first time")] private bool ensureDisplay;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            foreach (var feature in features)
            {
                // skip all disabled hints
                if (TutorialAssistManager.I.IsAssistDisabled(feature))
                    continue;

                TutorialAssistManager.I.ShowAssist(feature);

                if (feature.Ephemeral) TutorialAssistManager.I.DisableFeatureAssist(feature);

                // just display the first non-disabled hint
                return;
            }

            if (features.Length > 0 && ensureDisplay)
            {
                TutorialAssistManager.I.EnsureSeen(features[^1]);
            }
        }
    }
}