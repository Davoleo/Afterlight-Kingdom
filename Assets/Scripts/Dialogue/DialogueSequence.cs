using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Afterlight-Kingdom/Dialogue Sequence")]
    public class DialogueSequence : ScriptableObject
    {
        [Tooltip("Needed to persist completion in a save file; unused otherwise.")] [SerializeField]
        private string id;

        [Header("Dialogue")]
        [Tooltip("Spoken in order, one bubble per Interact press.")]
        [TextArea(1, 4)] [SerializeField]
        private string[] lines;

        [Header("Recap")]
        [Tooltip("Shown on every interaction after the sequence is done; one Interact press closes it.")]
        [TextArea(1, 4)] [SerializeField]
        private string summaryLine;

        public string Id => id;
        public string[] Lines => lines;
        public string SummaryLine => summaryLine;
    }
}
