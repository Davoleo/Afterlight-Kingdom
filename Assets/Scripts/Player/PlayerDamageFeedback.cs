using System.Collections;
using Controllers;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerDamageFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private KinematicCharacterMotor motor;
        [SerializeField] private HealthManager healthManager;
        [SerializeField] private PlayerCharacterController playerController;
        [SerializeField] private Image damageOverlay;
        [SerializeField] private Renderer[] characterRenderers;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 0.8f;
        [SerializeField] private float blinkInterval = 0.08f;

        [Header("Overlay")]
        [SerializeField] private float fadeInDuration = 0.08f;
        [SerializeField] private float holdDuration = 0.08f;
        [SerializeField] private float fadeOutDuration = 0.35f;
        [SerializeField] private float maxAlpha = 0.65f;

        [Header("Knockback")]
        [SerializeField] private float defaultKnockbackDistance = 1.2f;
        [SerializeField] private float knockbackDuration = 0.16f;

        private bool isInvincible;
        private Coroutine overlayCoroutine;
        private Coroutine invincibilityCoroutine;

        private void Awake()
        {
            if (motor == null)
                motor = GetComponent<KinematicCharacterMotor>();

            if (healthManager == null)
                healthManager = GetComponent<HealthManager>();

            if (playerController == null)
                playerController = GetComponent<PlayerCharacterController>();

            StopAllDamageFeedback();
        }

        private void Update()
        {
            if (healthManager != null && healthManager.Health <= 0)
                StopAllDamageFeedback();
        }

        public bool TryTakeDamage(
            int damage,
            Vector3 knockbackDirection,
            bool applyKnockback,
            float customKnockbackDistance = -1f)
        {
            if (isInvincible)
                return false;

            if (healthManager != null)
                healthManager.TakeDamage(damage);

            if (healthManager != null && healthManager.Health <= 0)
            {
                StopAllDamageFeedback();
                return true;
            }

            if (applyKnockback)
            {
                float finalDistance = customKnockbackDistance > 0f
                    ? customKnockbackDistance
                    : defaultKnockbackDistance;

                ApplyKnockback(knockbackDirection, finalDistance);
            }

            PlayDamageOverlay();
            StartInvincibility();

            return true;
        }

        public void StopAllDamageFeedback()
        {
            if (overlayCoroutine != null)
                StopCoroutine(overlayCoroutine);

            if (invincibilityCoroutine != null)
                StopCoroutine(invincibilityCoroutine);

            overlayCoroutine = null;
            invincibilityCoroutine = null;

            isInvincible = false;
            SetRenderersVisible(true);
            HideOverlay();

            if (playerController != null)
                playerController.StopExternalKnockback();
        }

        private void ApplyKnockback(Vector3 direction, float distance)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            direction.Normalize();

            if (playerController != null)
                playerController.ApplyExternalKnockback(direction, distance, knockbackDuration);
        }

        private void StartInvincibility()
        {
            if (invincibilityCoroutine != null)
                StopCoroutine(invincibilityCoroutine);

            invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
        }

        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;

            float elapsed = 0f;
            bool visible = true;

            while (elapsed < invincibilityDuration)
            {
                visible = !visible;
                SetRenderersVisible(visible);

                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            SetRenderersVisible(true);
            isInvincible = false;
            invincibilityCoroutine = null;
        }

        private void SetRenderersVisible(bool visible)
        {
            if (characterRenderers == null)
                return;

            foreach (Renderer renderer in characterRenderers)
            {
                if (renderer != null)
                    renderer.enabled = visible;
            }
        }

        private void PlayDamageOverlay()
        {
            if (damageOverlay == null)
                return;

            if (overlayCoroutine != null)
                StopCoroutine(overlayCoroutine);

            overlayCoroutine = StartCoroutine(DamageOverlayRoutine());
        }

        private IEnumerator DamageOverlayRoutine()
        {
            damageOverlay.gameObject.SetActive(true);

            yield return FadeOverlay(0f, maxAlpha, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return FadeOverlay(maxAlpha, 0f, fadeOutDuration);

            HideOverlay();
            overlayCoroutine = null;
        }

        private IEnumerator FadeOverlay(float fromAlpha, float toAlpha, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                SetOverlayAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));

                yield return null;
            }

            SetOverlayAlpha(toAlpha);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (damageOverlay == null)
                return;

            Color color = damageOverlay.color;
            color.a = alpha;
            damageOverlay.color = color;
        }

        private void HideOverlay()
        {
            if (damageOverlay == null)
                return;

            SetOverlayAlpha(0f);
            damageOverlay.gameObject.SetActive(false);
        }
    }
}