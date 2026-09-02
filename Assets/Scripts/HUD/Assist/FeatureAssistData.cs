using UnityEngine;

namespace HUD.Assist
{
    public enum AssistDismissMode
    {
        Timer,
        OnAction,
    }

    [CreateAssetMenu(fileName = "NewHintFeature", menuName = "Afterlight-Kingdom/Assist Data")]
    public class FeatureAssistData : ScriptableObject
    {
        [Tooltip("Needed to persist completion in a save file; unused otherwise.")] [SerializeField]
        private string id;

        [Header("Technical (bottom-center HUD)")] [TextArea(1, 3)] [SerializeField]
        private string technical;

        [Header("Narrative (speech bubble)")] [TextArea(1, 3)] [SerializeField]
        private string narrative;

        [Header("Dismissal")]
        [Tooltip("Timer: fades out on its own after duration. OnAction: stays on screen until the player performs the action")]
        [SerializeField]
        private AssistDismissMode dismissMode;

        [Header("Timing")] [Tooltip("Duration in seconds of the prompts")] [SerializeField]
        private float duration = 4f;

        [Header("Ephemeral Hint")]
        [Tooltip("will make this hint disappear forever after being displayed one time")]
        [SerializeField]
        private bool ephemeral;

        public string Id => id;
        public string TechicalPrompt => technical;
        public string NarrativePrompt => narrative;
        public AssistDismissMode DismissMode => dismissMode;
        public float DisplayDuration => duration;
        public bool Ephemeral => ephemeral;

    }
}