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
            if (save == null) return;
            coins = save.coins;
            keys = save.keys;
            collectedIds = save.collectedIds;
            RestoreCollectedState();
        }

        public void Collect(CollectibleType type, string id)
        {
            collectedIds.Add(id);
            switch (type)
            {
                case CollectibleType.Coin: coins++; break;
                case CollectibleType.Key:  keys++;  break;
            }
        }

        public int GetCount(CollectibleType type) => type switch
        {
            CollectibleType.Coin => coins,
            CollectibleType.Key  => keys,
            _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public bool UseKey()
        {
            if (keys <= 0) return false;
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
            foreach (var go in objects)
            {
                var handler = go.GetComponent<CollectibleTriggerHandler>();
                if (handler != null && collectedIds.Contains(handler.Id))
                    go.SetActive(false);
            }
        }
    }
}
