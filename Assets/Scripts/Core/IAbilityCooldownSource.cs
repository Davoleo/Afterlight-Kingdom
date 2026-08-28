namespace Core
{
    // Exposes an ability's cooldown/charge state for UI consumption (e.g. AbilityCooldownWidget),
    // decoupling the HUD from how each ability actually computes its own timing.
    public interface IAbilityCooldownSource
    {
        float Progress01 { get; }
        bool IsReady { get; }
        bool IsEngaged { get; }
    }
}
