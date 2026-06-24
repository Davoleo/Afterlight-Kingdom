using System;
using Gameplay;
using UnityEngine;

namespace Triggers
{
    public class CollectibleTriggerHandler : MonoBehaviour
    {
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType collectibleType = CollectibleType.Coin;

        [Header("Ability Settings")]
        [SerializeField] private AbilityType abilityToUnlock = AbilityType.Dash;

        public string Id =>
            $"{transform.position.x}_{transform.position.y}_{transform.position.z}";

        private CollectiblesManager _collectiblesManager;
        private AbilityManager _abilityManager;

        private void Start()
        {
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
            _collectiblesManager = gameManager.GetComponent<CollectiblesManager>();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            _abilityManager = player.GetComponent<AbilityManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player"))
                return;

            if (_collectiblesManager.collectedIds.Contains(Id))
                return;

            CollectibleType resolvedType = ResolveCollectibleType();

            _collectiblesManager.Collect(resolvedType, Id);

            if (resolvedType == CollectibleType.Ability)
            {
                if (_abilityManager == null)
                    throw new NullReferenceException("AbilityManager missing on Player.");

                _abilityManager.UnlockAbility(abilityToUnlock);
            }

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