using Sound;
using Triggers;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay
{
    public class Level2GateController : MonoBehaviour
    {
        [SerializeField] private AnimationClip gateOpenClip;
        [SerializeField] private AudioClip gateOpenAudio;
        [SerializeField] private Animator animator;
        private PlayableGraph _graph;

        private int _leversFlicked;

        private void Start()
        {
            LeverInteractionHandler.LeverStateChanged += OnLeverStatusChange;
        }

        private void OpenGate()
        {
            if (_graph.IsValid()) _graph.Destroy();
            AnimationPlayableUtilities.PlayClip(animator, gateOpenClip, out _graph);
            AudioManager.Instance.PlaySfx(gateOpenAudio);
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