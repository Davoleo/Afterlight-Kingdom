using Gameplay;
using UnityEngine;

namespace Triggers
{
    public class CollectibleTriggerHandler : MonoBehaviour
    {
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType collectibleType = CollectibleType.Coin;

        public string Id =>
            $"{transform.position.x}_{transform.position.y}_{transform.position.z}";

        private CollectiblesManager _collectiblesManager;

        private void Start()
        {
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
            _collectiblesManager = gameManager.GetComponent<CollectiblesManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player"))
                return;

            if (_collectiblesManager.collectedIds.Contains(Id))
                return;

            CollectibleType resolvedType = ResolveCollectibleType();

            _collectiblesManager.Collect(resolvedType, Id);

            gameObject.SetActive(false);
        }

        private CollectibleType ResolveCollectibleType()
        {
            if (gameObject.CompareTag("Coins"))
                return CollectibleType.Coin;

            if (gameObject.CompareTag("Keys"))
                return CollectibleType.Key;

            return collectibleType;
        }
    }
}