using UnityEngine;

namespace Core
{
    // Generic externally-driven progress gauge. Unlike Cooldown (which ticks itself down over
    // time), this just holds whatever progress/ready/engaged state its owner pushes into it each
    // frame - for abilities whose "charge" is driven by something else (e.g. an Animator state)
    // rather than a plain timer.
    public class AbilityGauge : IAbilityCooldownSource
    {
        public float Progress01 { get; private set; }
        public bool IsReady { get; private set; }
        public bool IsEngaged { get; set; }

        public void Set(float progress01, bool isReady)
        {
            Progress01 = Mathf.Clamp01(progress01);
            IsReady = isReady;
        }
    }
}
