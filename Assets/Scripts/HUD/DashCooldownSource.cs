using Core;
using Gameplay;
using Player;
using UnityEngine;

namespace HUD
{
    // Adapts PlayerCharacterController's dash Cooldown to IAbilityCooldownSource for
    // AbilityCooldownWidget, so the controller itself doesn't need to know about HUD concerns.
    // Also hides the overlay entirely while the Dash ability isn't unlocked yet.
    public class DashCooldownSource : MonoBehaviour, IAbilityCooldownSource
    {
        private PlayerCharacterController _controller;
        private AbilityManager _abilityManager;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            _controller = player.GetComponent<PlayerCharacterController>();
            _abilityManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AbilityManager>();
        }

        private bool HasAbility => _abilityManager.HasAbility(AbilityType.Dash);

        public float Progress01 => HasAbility ? _controller.DashCooldown.Progress01 : 0f;
        public bool IsReady => !HasAbility || _controller.DashCooldown.IsReady;
        public bool IsEngaged => HasAbility && _controller.DashCooldown.IsEngaged;
    }
}
