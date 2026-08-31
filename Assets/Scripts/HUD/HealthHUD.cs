using System.Collections.Generic;
using Player;
using UnityEngine;

namespace HUD
{
    public class HealthHUD : MonoBehaviour
    {
        internal static readonly Dictionary<HeartEnum, Sprite> HeartSprites = new();

        [SerializeField] private HealthManager healthManager;

        private Heart[] _hearts;
        private int _prevHealth;

        private void Start()
        {
            _hearts = gameObject.GetComponentsInChildren<Heart>(true);
            HeartSprites.Add(HeartEnum.Empty, Resources.Load<Sprite>("Sprites/Heart Empty"));
            HeartSprites.Add(HeartEnum.Full, Resources.Load<Sprite>("Sprites/Heart Full"));
        }

        private void Update()
        {
            if (_prevHealth == healthManager.Health)
                return;

            var log = "Health: ";

            int heart = 0;
            while (heart < HealthManager.MaxHealth)
            {
                log += heart < healthManager.Health ? 'o' : 'x';

                _hearts[heart].SetState(heart < healthManager.Health ? HeartEnum.Full : HeartEnum.Empty);
                heart++;
            }

            Debug.Log(log);

            _prevHealth = healthManager.Health;
        }
    }

    internal enum HeartEnum
    {
        Empty = 0,
        Full = 2
    }
}
