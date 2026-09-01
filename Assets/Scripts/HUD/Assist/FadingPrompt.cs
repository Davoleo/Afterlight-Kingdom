using System.Collections;
using TMPro;
using UnityEngine;

namespace HUD.Assist
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadingPrompt : MonoBehaviour
    {
        private TMP_Text _label;
        [SerializeField] private float fadeIn = 0.2f;
        [SerializeField] private float fadeOut = 0.2f;

        private CanvasGroup _canvasGroup;
        private Coroutine _activeRoutine;

        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        public void Show(string text, float holdDuration)
        {
            if (_activeRoutine is not null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(ShowRoutine(text, holdDuration));
        }

        public void ShowHeld(string text)
        {
            if (_activeRoutine is not null) StopCoroutine(_activeRoutine);
            _label.text = text;
            _activeRoutine = StartCoroutine(FadeRoutine(_canvasGroup.alpha, 1f, fadeIn));
        }

        public void Hide()
        {
            if (_activeRoutine is not null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(FadeRoutine(_canvasGroup.alpha, 0f, fadeOut));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, duration <= 0f ? 1f : t / duration);
                yield return null;
            }

            _canvasGroup.alpha = to;
        }

        private IEnumerator ShowRoutine(string text, float holdDuration)
        {
            _label.text = text;
            yield return FadeRoutine(_canvasGroup.alpha, 1f, fadeIn);
            yield return new WaitForSeconds(holdDuration);
            yield return FadeRoutine(_canvasGroup.alpha, 0f, fadeOut);
            _activeRoutine = null;
        }
    }
}