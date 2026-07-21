using UnityEngine;

namespace VisualEffects
{
    [RequireComponent(typeof(Light))]
    public class TorchLightFlicker : MonoBehaviour
    {
        [Header("Intensity")]
        [SerializeField] private float baseIntensity = 2f;
        [SerializeField] private float intensityVariation = 0.4f;

        [Header("Range")]
        [SerializeField] private float baseRange = 5f;
        [SerializeField] private float rangeVariation = 0.35f;

        [Header("Flicker")]
        [SerializeField] private float flickerSpeed = 8f;

        private Light _torchLight;
        private float _noiseOffset;

        private void Awake()
        {
            _torchLight = GetComponent<Light>();
            _noiseOffset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(
                _noiseOffset,
                Time.time * flickerSpeed
            );

            float centeredNoise = (noise - 0.5f) * 2f;

            _torchLight.intensity =
                baseIntensity + centeredNoise * intensityVariation;

            _torchLight.range =
                baseRange + centeredNoise * rangeVariation;
        }
    }
}