using HUD.Assist;
using UnityEngine;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class AssistTriggerZone : MonoBehaviour
    {
        [SerializeField] private FeatureAssistData feature;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                TutorialAssistManager.Instance.ShowAssist(feature);

            TutorialAssistManager.Instance.DisableFeatureAssist(feature);
        }
    }
}