using UnityEngine;

namespace HUD.Assist
{
    public class AssistCompletionZone : MonoBehaviour
    {
        public FeatureAssistData feature;

        private void OnTriggerEnter(Collider other) => TutorialAssistManager.I.DisableFeatureAssist(feature);
    }
}