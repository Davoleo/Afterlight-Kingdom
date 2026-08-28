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
        private float _readySince = -1f;

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

            bool shouldShow = _source.IsEngaged || !_source.IsReady;

            if (shouldShow)
            {
                _readySince = -1f;
            }
            else
            {
                if (_readySince < 0f)
                    _readySince = Time.time;

                shouldShow = Time.time - _readySince < hideDelayAfterReady;
            }

            _canvasGroup.alpha = shouldShow ? 1f : 0f;
            _canvasGroup.blocksRaycasts = shouldShow;
            _canvasGroup.interactable = shouldShow;
        }
    }
}
