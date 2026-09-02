using UnityEngine;

namespace HUD.Assist
{
    [RequireComponent(typeof(FadingPrompt))]
    public class SpeechBubble : MonoBehaviour
    {
        [Tooltip("Camera to billboard against. Empty to use main camera.")] [SerializeField]
        private Camera targetCamera;

        private FadingPrompt _prompt;

        private void Awake()
        {
            _prompt = GetComponent<FadingPrompt>();
        }

        private void Start()
        {
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (!targetCamera) return; // If no MainCamera is found
            
            //billboard
            transform.rotation = targetCamera.transform.rotation;
        }

        public void Show(string text, float duration) => _prompt.Show(text, duration);

        public void ShowHeld(string text) => _prompt.ShowHeld(text);

        public void Hide() => _prompt.Hide();
    }

}