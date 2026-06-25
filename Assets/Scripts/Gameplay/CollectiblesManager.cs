using System.Collections.Generic;
using Core;
using Triggers;
using UnityEngine;

namespace Gameplay
{
    public enum CollectibleType
    {
        Coin,
        Key,
        Ability
    }

    public struct Collectibles
    {
        public List<GameObject> Coins;
        public List<GameObject> Keys;
    }

    public class CollectiblesManager : MonoBehaviour
    {
        public Collectibles Collectibles;

        public int coins;
        public int keys;

        public List<string> collectedIds = new List<string>();

        private void Start()
        {
            Collectibles.Coins = new List<GameObject>();
            Collectibles.Keys = new List<GameObject>();

            Collectibles.Coins.AddRange(GameObject.FindGameObjectsWithTag("Coins"));
            Collectibles.Keys.AddRange(GameObject.FindGameObjectsWithTag("Keys"));

            var save = SaveManager.Load();

            if (SaveManager.HasSave)
            {
                coins = save.coins;
                keys = save.keys;

                if (save.collectedIds != null)
                    collectedIds = new List<string>(save.collectedIds);
            }

            RestoreCollectedState();
        }

        public void Collect(CollectibleType type, string id)
        {
            bool isNewCollectible = RegisterCollectedId(id);

            if (!isNewCollectible)
                return;

            switch (type)
            {
                case CollectibleType.Coin:
                    coins++;
                    break;

                case CollectibleType.Key:
                    keys++;
                    break;
            }
        }

        public bool RegisterCollectedId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            collectedIds ??= new List<string>();

            if (collectedIds.Contains(id))
                return false;

            collectedIds.Add(id);
            return true;
        }

        public bool IsCollected(string id)
        {
            return collectedIds != null && collectedIds.Contains(id);
        }

        public int GetCount(CollectibleType type)
        {
            return type switch
            {
                CollectibleType.Coin => coins,
                CollectibleType.Key => keys,
                _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public bool UseKey()
        {
            if (keys <= 0)
                return false;

            keys--;
            return true;
        }

        private void RestoreCollectedState()
        {
            DeactivateMatching(Collectibles.Coins);
            DeactivateMatching(Collectibles.Keys);
        }

        private void DeactivateMatching(List<GameObject> objects)
        {
            foreach (GameObject go in objects)
            {
                CollectibleTriggerHandler handler = go.GetComponent<CollectibleTriggerHandler>();

                if (handler != null && IsCollected(handler.Id))
                    go.SetActive(false);
            }
        }
    }
}