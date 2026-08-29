using Core;
using Gameplay;
using Player;
using UnityEngine;

namespace HUD
{
    // Adapts BowController's draw AbilityGauge to IAbilityCooldownSource for
    // AbilityCooldownWidget, so the controller itself doesn't need to know about HUD concerns.
    // Also hides the overlay entirely while the Bow ability isn't unlocked yet.
    public class BowDrawProgressSource : MonoBehaviour, IAbilityCooldownSource
    {
        private BowController _controller;
        private AbilityManager _abilityManager;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            _controller = player.GetComponent<BowController>();
            _abilityManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AbilityManager>();
        }

        private bool HasAbility => _abilityManager.HasAbility(AbilityType.Bow);

        public float Progress01 => HasAbility ? _controller.DrawGauge.Progress01 : 0f;
        public bool IsReady => !HasAbility || _controller.DrawGauge.IsReady;
        public bool IsEngaged => HasAbility && _controller.DrawGauge.IsEngaged;
    }
}
