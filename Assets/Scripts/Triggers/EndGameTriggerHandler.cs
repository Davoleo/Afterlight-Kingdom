using Core;
using UnityEngine;

namespace Triggers
{
    // Sits on the Throne trigger collider. Fires the end-game cutscene once, the first time
    // the player reaches it during real play - mirrors CheckpointTriggerHandler's guard against
    // firing while the level is still loading additively on top of Core.
    public class EndGameTriggerHandler : MonoBehaviour
    {
        [SerializeField] private EndGameSequence endGameSequence;

        private bool _fired;

        private void OnTriggerEnter(Collider other)
        {
            if (_fired) return;
            if (!other.CompareTag("Player")) return;
            if (GameStateManager.Current != GameState.Playing) return;

            _fired = true;
            GetComponent<Collider>().enabled = false;

            endGameSequence.Play();
        }
    }
}
