using Core;
using Player;
using UnityEngine;

namespace HUD
{
    // Adapts PlayerCharacterController's dash Cooldown to IAbilityCooldownSource for
    // AbilityCooldownWidget, so the controller itself doesn't need to know about HUD concerns.
    public class DashCooldownSource : MonoBehaviour, IAbilityCooldownSource
    {
        private PlayerCharacterController _controller;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            _controller = player.GetComponent<PlayerCharacterController>();
        }

        public float Progress01 => _controller.DashCooldown.Progress01;
        public bool IsReady => _controller.DashCooldown.IsReady;
        public bool IsEngaged => _controller.DashCooldown.IsEngaged;
    }
}
