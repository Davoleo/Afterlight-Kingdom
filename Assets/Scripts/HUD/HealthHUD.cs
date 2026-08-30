using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class HealthHUD : MonoBehaviour
    {
        [SerializeField] // UI elements
        private Image[] hearts;
        
        private readonly Dictionary<HeartEnum, Sprite> _heartSprites = new();

        private HealthManager _healthManager;

        private void Start()
        {
            _healthManager =  GameObject.FindGameObjectWithTag("Player").GetComponent<HealthManager>();
            _heartSprites.Add(HeartEnum.Empty, Resources.Load<Sprite>("Sprites/Heart Empty"));
            _heartSprites.Add(HeartEnum.Full, Resources.Load<Sprite>("Sprites/Heart Full"));
        }

        private void Update()
        {
            int heart = 0;
            while (heart < HealthManager.MaxHealth)
            {
                hearts[heart].sprite = heart < _healthManager.Health ? _heartSprites[HeartEnum.Full] : _heartSprites[HeartEnum.Empty];
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
