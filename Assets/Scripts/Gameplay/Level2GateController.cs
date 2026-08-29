using Triggers;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay
{
    public class Level2GateController : MonoBehaviour
    {
        [SerializeField] private AnimationClip gateOpenClip;
        private PlayableGraph _graph;

        private int _leversFlicked;

        private void OpenGate()
        {
            if (_graph.IsValid()) _graph.Destroy();
            AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), gateOpenClip, out _graph);
        }

        private void Start()
        {
            LeverInteractionHandler.LeverStateChanged += OnLeverStatusChange;
        }

        private void OnLeverStatusChange(bool active)
        {
            // gate should be already open levers are now dead switches
            if (_leversFlicked == 3) return;

            _leversFlicked += active ? 1 : -1;

            // first time 3 levers are on contemporarily -> open gate
            if (_leversFlicked == 3) OpenGate();
        }

        private void OnDestroy()
        {
            if (_graph.IsValid()) _graph.Destroy();
        }
    }
}