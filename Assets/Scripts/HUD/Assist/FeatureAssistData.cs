using UnityEngine;

namespace HUD.Assist
{
    [CreateAssetMenu(fileName = "NewHintFeature", menuName = "Kingdom-Afterlight/Hints/Feature Data")]
    public class FeatureAssistData : ScriptableObject
    {
        [Tooltip("Needed to persist completion in a save file; unused otherwise.")] [SerializeField]
        private string id;

        [Header("Technical (bottom-center HUD)")] [TextArea(1, 2)] [SerializeField]
        private string technical;

        [Header("Narrative (speech bubble)")] [TextArea(2, 4)] [SerializeField]
        private string narrative;

        [Header("Timing")] [Tooltip("Duration in seconds of the prompts")] [SerializeField]
        private float duration = 4f;

        public string Id => id;
        public string TechicalPrompt => technical;
        public string NarrativePrompt => narrative;
        public float DisplayDuration => duration;

    }
}