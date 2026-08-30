using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class HealthHUD : MonoBehaviour
    {
        [SerializeField]
        private HealthManager healthManager;
        
        private Dictionary<HeartEnum, Sprite> heartSprites = new();

        // UI elements
        private Image[] _hearts;

        private void Start()
        {
            _hearts = gameObject.GetComponentsInChildren<Image>();
            
            heartSprites.Add(HeartEnum.Empty, Resources.Load<Sprite>("Sprites/heart_empty"));
            heartSprites.Add(HeartEnum.Full, Resources.Load<Sprite>("Sprites/heart_full"));
        }

        private void Update()
        {
            int heart = 0;
            while (heart < HealthManager.MaxHealth)
            {
                _hearts[heart].sprite = heart < healthManager.Health ? heartSprites[HeartEnum.Full] : heartSprites[HeartEnum.Empty];
                heart++;
            }
        }
    }

    internal enum HeartEnum
    {
        Empty = 0,
        Full = 2
    }
}
