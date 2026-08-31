using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class Heart : MonoBehaviour
    {
        private Image image;

        private void Start()
        {
            image = GetComponent<Image>();
            //image.sprite = HealthHUD.HeartSprites[HeartEnum.Full];
        }

        internal void SetState(HeartEnum state)
        {
            image.sprite = HealthHUD.HeartSprites[state];

        }
    }
}