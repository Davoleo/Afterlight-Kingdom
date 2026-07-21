using System.Collections;
using UnityEngine;

namespace VisualEffects
{
    [RequireComponent(typeof(Renderer))]
    public class CheckpointRingEffect : MonoBehaviour
    {
        //Extract ring properties
        private static readonly int RingRadiusId =
            Shader.PropertyToID("_RingRadius");

        private static readonly int OpacityId =
            Shader.PropertyToID("_Opacity");

        private static readonly int GlowIntensityId =
            Shader.PropertyToID("_GlowIntensity");

        [Header("Animation")]
        [SerializeField, Min(0.01f)]
        private float duration = 0.8f;

        [SerializeField, Range(0f, 0.5f)]
        private float startRadius = 0.08f;

        [SerializeField, Range(0f, 0.5f)]
        private float endRadius = 0.45f;

        [SerializeField, Min(0f)]
        private float maximumGlowIntensity = 4f;

        [Header("Animation Curves")]
        [SerializeField]
        private AnimationCurve expansionCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve fadeCurve =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Behaviour")]
        [Tooltip("Se attivo, il checkpoint mostra l'effetto una sola volta.")]
        [SerializeField]
        private bool playOnce = true;

        [Tooltip("Disabilita il Renderer quando l'anello è invisibile.")]
        [SerializeField]
        private bool disableRendererWhenHidden = true;

        private Renderer _ringRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _animationCoroutine;
        private bool _hasPlayed;

        private void Start()
        {
            _ringRenderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            //idle --> invisible ring
            ApplyShaderValues(
                radius: startRadius,
                opacity: 0f,
                glowIntensity: 0f
            );

            if (disableRendererWhenHidden)
            {
                _ringRenderer.enabled = false;
            }
        }

        //Ring activation
        public void Play()
        {
            if (playOnce && _hasPlayed)
            {
                return;
            }

            _hasPlayed = true;

            //stop previous animation and restart
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            _ringRenderer.enabled = true;
            _animationCoroutine = StartCoroutine(AnimateRing());
        }

        public void ResetEffect()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            _hasPlayed = false;

            ApplyShaderValues(
                radius: startRadius,
                opacity: 0f,
                glowIntensity: 0f
            );

            if (disableRendererWhenHidden)
            {
                _ringRenderer.enabled = false;
            }
        }

        private IEnumerator AnimateRing()
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime =
                    Mathf.Clamp01(elapsedTime / duration);

                // Increasing radius
                float expansionProgress =
                    expansionCurve.Evaluate(normalizedTime);

                // Increasing fade
                float fadeProgress =
                    fadeCurve.Evaluate(normalizedTime);

                float currentRadius = Mathf.Lerp(
                    startRadius,
                    endRadius,
                    expansionProgress
                );

                float currentOpacity =
                    1f - fadeProgress;

                float currentGlow = Mathf.Lerp(
                    maximumGlowIntensity,
                    0f,
                    fadeProgress
                );

                ApplyShaderValues(
                    currentRadius,
                    currentOpacity,
                    currentGlow
                );

                yield return null;
            }

            ApplyShaderValues(
                radius: endRadius,
                opacity: 0f,
                glowIntensity: 0f
            );

            if (disableRendererWhenHidden)
            {
                _ringRenderer.enabled = false;
            }

            _animationCoroutine = null;
        }

        private void ApplyShaderValues(
            float radius,
            float opacity,
            float glowIntensity)
        {
            _propertyBlock.SetFloat(RingRadiusId, radius);
            _propertyBlock.SetFloat(OpacityId, opacity);
            _propertyBlock.SetFloat(
                GlowIntensityId,
                glowIntensity
            );

            _ringRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}