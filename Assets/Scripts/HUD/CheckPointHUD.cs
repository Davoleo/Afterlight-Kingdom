using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace HUD
{
    public class CheckPointHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI savedMessage;
        
        public void ShowSavedMessage() => StartCoroutine(FadeSavedMessage());
        
        private IEnumerator FadeSavedMessage()
        {
            // Fade In
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.3f;
                savedMessage.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            
            // Hold
            yield return new WaitForSeconds(1.5f);
            
            // Fade Out
            t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime / 0.5f;
                savedMessage.alpha = Mathf.Clamp01(t);
                yield return null;
            }
        }
    }
}