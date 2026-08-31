using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class Heart : MonoBehaviour
    {
        private Image _image;

        private void Start() => _image = GetComponent<Image>();

        internal void SetState(HeartEnum state) => _image.sprite = HealthHUD.HeartSprites[state];
    }
}