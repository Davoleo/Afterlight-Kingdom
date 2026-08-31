using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    // Full-screen progress UI shown during scene transitions. Lives on a
    // DontDestroyOnLoad prefab (Resources/LoadingScreen) so it survives the hard
    // MainMenu -> Core scene swap, and doubles as the persistent coroutine host for
    // SceneTransitions flows (a flow started on a scene object would be killed when
    // that scene unloads mid-transition).
    
    // The root GameObject stays active for its entire life so it can host coroutines
    // and run Update; "hidden" means the Canvas component is disabled.
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Slider bar;
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float minVisibleTime = 0.5f;
        [SerializeField] private float barLerpSpeed = 3f;

        private static LoadingScreen _instance;
        private float _barTarget;
        private float _shownAtRealtime;
        
        private const string LoadingScreenName = "LoadingScreen";

        // Singleton
        public static LoadingScreen Instance
        {
            get
            {
                if (_instance is not null) return _instance;

                _instance = FindAnyObjectByType<LoadingScreen>();
                if (_instance is null)
                {
                    var prefab = Resources.Load<LoadingScreen>(LoadingScreenName);
                    _instance = Instantiate(prefab);
                }

                _instance.name = LoadingScreenName;
                DontDestroyOnLoad(_instance.gameObject);
                _instance.ForceHide();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance is not null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            bar.value = Mathf.MoveTowards(bar.value, _barTarget, barLerpSpeed * Time.unscaledDeltaTime);
        }

        public void Report(float progress) => _barTarget = Mathf.Clamp01(progress);

        public IEnumerator Show()
        {
            _shownAtRealtime = Time.realtimeSinceStartup;
            _barTarget = 0f;
            bar.value = 0f;
            canvas.enabled = true;
            // Avoid pressing menu buttons during loading.
            group.blocksRaycasts = true;
            yield return Fade(0f, 1f);
        }

        public IEnumerator Hide()
        {
            float elapsed = Time.realtimeSinceStartup - _shownAtRealtime;
            if (elapsed < minVisibleTime)
                yield return new WaitForSecondsRealtime(minVisibleTime - elapsed);

            yield return Fade(1f, 0f);
            ForceHide();
        }

        private void ForceHide()
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            canvas.enabled = false;
            _barTarget = 0f;
            bar.value = 0f;
        }

        private IEnumerator Fade(float from, float to)
        {
            group.alpha = from;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
