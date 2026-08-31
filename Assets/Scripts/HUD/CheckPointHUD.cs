using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class CheckPointHUD : MonoBehaviour
    {
        [SerializeField] private Image savedMessage;

        public void ShowSavedMessage() => StartCoroutine(FadeSavedMessage());

        private void Start()
        {
            SetAlpha(0f);
        }

        private IEnumerator FadeSavedMessage()
        {
            // Fade In
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.3f;
                SetAlpha(Mathf.Clamp01(t));
                yield return null;
            }

            // Hold
            yield return new WaitForSeconds(1.5f);

            // Fade Out
            t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime / 0.5f;
                SetAlpha(Mathf.Clamp01(t));
                yield return null;
            }
        }

        private void SetAlpha(float alpha)
        {
            Color c = savedMessage.color;
            c.a = alpha;
            savedMessage.color = c;
        }
    }
}
