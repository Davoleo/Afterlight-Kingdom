using System;
using Gameplay;
using UnityEngine;

namespace Triggers
{
    public class CollectibleTriggerHandler : MonoBehaviour
    {
        public string Id =>
            $"{transform.position.x}_{transform.position.y}_{transform.position.z}";

        private CollectiblesManager _manager;

        private void Start()
        {
            _manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CollectiblesManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;

            gameObject.SetActive(false);

            CollectibleType type = gameObject.tag switch
            {
                "Coins" => CollectibleType.Coin,
                "Keys" => CollectibleType.Key,
                _ => throw new ArgumentOutOfRangeException()
            };
            _manager.Collect(type, Id);
        }
    }
}
