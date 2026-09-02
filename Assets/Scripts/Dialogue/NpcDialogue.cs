using HUD.Assist;
using Player;
using UnityEngine;

namespace Dialogue
{
    /// <summary>
    /// Drives an NPC speech bubble through a <see cref="DialogueSequence"/>.
    /// Each Interact press advances one line; the last line closes the bubble.
    /// Interacting again after that shows a single recap line, which closes on
    /// the next press. State is in-memory only and resets on scene reload.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class NpcDialogue : MonoBehaviour
    {
        private enum Talk
        {
            NotStarted,
            InProgress,
            Completed,
        }

        [Tooltip("Lines and recap this NPC speaks.")] [SerializeField]
        private DialogueSequence sequence;

        [Tooltip("World-space bubble on this NPC.")] [SerializeField]
        private SpeechBubble bubble;

        [Tooltip("Shown while the player is in range and no dialogue is open.")] [SerializeField]
        private string prompt = "!";

        private PlayerCharacterController _player;

        private Talk _state = Talk.NotStarted;
        private int _line;
        private bool _summaryShown;
        private bool _playerInRange;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
        }

        private void Update()
        {
            if (!_playerInRange) return;

            if (!CommandUtils.IsUp(_player.triggers, PlayerTrigger.Interact)) return;
            CommandUtils.Off(ref _player.triggers, PlayerTrigger.Interact);

            switch (_state)
            {
                case Talk.NotStarted:
                    _state = Talk.InProgress;
                    _line = 0;
                    ShowLineOrComplete();
                    break;

                case Talk.InProgress:
                    _line++;
                    ShowLineOrComplete();
                    break;

                case Talk.Completed:
                    if (!_summaryShown)
                    {
                        _summaryShown = true;
                        bubble.ShowHeld(sequence.SummaryLine);
                    }
                    else
                    {
                        _summaryShown = false;
                        bubble.Hide();
                    }

                    break;
            }
        }

        private void ShowLineOrComplete()
        {
            string[] lines = sequence.Lines;
            if (lines is not null && _line < lines.Length)
            {
                bubble.ShowHeld(lines[_line]);
                return;
            }

            // Past the last line: the sequence is done, so close the bubble.
            _state = Talk.Completed;
            _summaryShown = false;
            bubble.Hide();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = true;

            if (_state == Talk.InProgress) bubble.ShowHeld(sequence.Lines[_line]); // resume where we left off
            else bubble.ShowHeld(prompt);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInRange = false;
            bubble.Hide();

            // Let the recap open fresh next visit; keep _line so InProgress resumes.
            if (_state == Talk.Completed) _summaryShown = false;
        }
    }
}
