using Core;
using Player;
using UnityEngine;

namespace HUD
{
    // Adapts BowController's draw AbilityGauge to IAbilityCooldownSource for
    // AbilityCooldownWidget, so the controller itself doesn't need to know about HUD concerns.
    public class BowDrawProgressSource : MonoBehaviour, IAbilityCooldownSource
    {
        private BowController _controller;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            _controller = player.GetComponent<BowController>();
        }

        public float Progress01 => _controller.DrawGauge.Progress01;
        public bool IsReady => _controller.DrawGauge.IsReady;
        public bool IsEngaged => _controller.DrawGauge.IsEngaged;
    }
}
