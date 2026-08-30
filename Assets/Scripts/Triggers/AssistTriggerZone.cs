using HUD.Assist;
using UnityEngine;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class AssistTriggerZone : MonoBehaviour
    {
        [SerializeField] private FeatureAssistData feature;
        [SerializeField] private bool ephemeral = false;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                TutorialAssistManager.Instance.ShowAssist(feature);
            
            if (ephemeral) TutorialAssistManager.Instance.DisableFeatureAssist(feature);
        }
    }
}