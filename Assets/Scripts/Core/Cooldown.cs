using System;
using UnityEngine;

namespace Core
{
    // Self-ticking ability cooldown. Any ability that needs a "used it, wait N seconds"
    // timer can declare a field of this type instead of handling duration/timer fields.
    [Serializable]
    public class Cooldown : IAbilityCooldownSource
    {
        [SerializeField] private float duration;
        private float _timer;

        public Cooldown(float duration)
        {
            this.duration = duration;
        }

        public float Progress01 => duration <= 0f ? 1f : 1f - Mathf.Clamp01(_timer / duration);
        public bool IsReady => _timer <= 0f;
        public bool IsEngaged => false;

        // Update the timer, this is not a MonoBehaviour script, it does not own a Update() method, so the update has
        // to be explicit.
        public void Tick(float deltaTime) => _timer = Mathf.Max(0f, _timer - deltaTime);

        public void Trigger() => _timer = duration;
    }
}
