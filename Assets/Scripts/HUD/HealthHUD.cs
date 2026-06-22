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
            heartSprites.Add(HeartEnum.Half, Resources.Load<Sprite>("Sprites/heart_half"));
            heartSprites.Add(HeartEnum.Full, Resources.Load<Sprite>("Sprites/heart_full"));
        }

        private void Update()
        {
            int fullHearts = (healthManager.Health / 2);
            for (int i = 0; i < fullHearts; i++)
            {
                _hearts[i].sprite = heartSprites[HeartEnum.Full];
            }

            if (healthManager.Health < 6)
            {
                // odd health value = half-heart | even health value = full heart
                int lastFullness = healthManager.Health % 2;
                _hearts[fullHearts].sprite = heartSprites[(HeartEnum) lastFullness];
            }
        }
    }

    internal enum HeartEnum
    {
        Empty = 0,
        Half = 1,
        Full = 2
    }
}
