using Core;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    // Generic radial cooldown overlay. Reads an IAbilityCooldownSource each frame and drives
    // a Filled/Radial360 Image; visual appearance (sprites, colors, ring style) is prefab-only.
    // Reused for both the dash and bow-draw overlays - see DashCooldownSource and
    // BowDrawProgressSource for the adapters that expose each ability as a source.
    public class AbilityCooldownWidget : MonoBehaviour
    {
        [SerializeField] private Component source;
        [SerializeField] private Image fillRing;
        [SerializeField] private Color chargingColor = Color.white;
        [SerializeField] private Color readyColor = Color.green;

        [Tooltip("Seconds to keep showing the overlay after it becomes ready (while not engaged). 0 = hide immediately.")]
        [SerializeField] private float hideDelayAfterReady;

        private IAbilityCooldownSource _source;
        private CanvasGroup _canvasGroup;
        private bool _wasShownLastFrame;
        private float _hideAt = -1f;

        private void Awake()
        {
            _source = source as IAbilityCooldownSource;
            if (_source == null)
                Debug.LogError($"{name}: assigned source does not implement IAbilityCooldownSource", this);

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (_source == null) return;

            fillRing.fillAmount = _source.Progress01;
            fillRing.color = _source.IsReady ? readyColor : chargingColor;

            bool wantsVisible = _source.IsEngaged || !_source.IsReady;

            if (wantsVisible)
            {
                // Actively charging/engaged - no linger pending.
                _hideAt = -1f;
            }
            else if (_wasShownLastFrame && _hideAt < 0f)
            {
                // Just transitioned from shown to ready+idle - start the linger window.
                // Only do this on that transition, not whenever we happen to observe
                // "ready" (e.g. on the very first frame with no ability unlocked at all,
                // which must hide immediately, not linger)
                _hideAt = Time.time + hideDelayAfterReady;
            }

            bool shouldShow = wantsVisible || (_hideAt >= 0f && Time.time < _hideAt);
            _wasShownLastFrame = shouldShow;

            _canvasGroup.alpha = shouldShow ? 1f : 0f;
            _canvasGroup.blocksRaycasts = shouldShow;
            _canvasGroup.interactable = shouldShow;
        }
    }
}
