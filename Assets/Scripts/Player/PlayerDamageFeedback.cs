using System.Collections;
using Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerDamageFeedback : MonoBehaviour
    {
        [Header("References")]
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

        //limit maximum knockback distance to avoid exaggerated pushes
        // Increased to allow the charging enemy to apply a stronger knockback
        [SerializeField] private float maxKnockbackDistance = 6f;

        private bool isInvincible;
        private Coroutine overlayCoroutine;
        private Coroutine invincibilityCoroutine;
        private PlayerCharacterController playerController;
        public bool IsInvincible => isInvincible;

        private void Start()
        {
            playerController = GetComponent<PlayerCharacterController>();

            StopAllDamageFeedback();
        }

        public void PlayDamageFeedback(Vector3 knockbackDirection, bool applyKnockback, float customKnockbackDistance = -1f)
        {
            if (applyKnockback)
            {
                float finalDistance = customKnockbackDistance > 0f ? customKnockbackDistance : defaultKnockbackDistance;

                finalDistance = GetControlledKnockbackDistance(finalDistance);

                ApplyKnockback(knockbackDirection, finalDistance);
            }

            PlayDamageOverlay();
            StartInvincibility();
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

            playerController.StopExternalKnockback();
        }

        private float GetControlledKnockbackDistance(float requestedDistance)
        {
            //clamp the requested knockback, but do not force it to be always weak
            float controlledDistance = Mathf.Clamp(requestedDistance, 0f, maxKnockbackDistance);

            return controlledDistance;
        }

        private void ApplyKnockback(Vector3 direction, float distance)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            if (distance <= 0f)
                return;

            direction.Normalize();

            //stop previous external knockback before applying a new one
            //this prevents knockback stacking
            playerController.StopExternalKnockback();

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
            foreach (Renderer renderer in characterRenderers)
            {
                renderer.enabled = visible;
            }
        }

        private void PlayDamageOverlay()
        {
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
            Color color = damageOverlay.color;
            color.a = alpha;
            damageOverlay.color = color;
        }

        private void HideOverlay()
        {
            SetOverlayAlpha(0f);
            damageOverlay.gameObject.SetActive(false);
        }
    }
}