using System;
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
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public bool UseKey()
        {
            if (keys <= 0)
                return false;

            keys--;
            return true;
        }

        // Called explicitly by CoreLoader once the level has actually finished loading and
        // become the active scene - same reasoning as CheckpointManager.Respawn(): the level's
        // Coin/Key objects don't exist yet while this GameObject is still booting inside Core.
        public void RestoreFromSave(SaveData save)
        {
            coins = save.coins;
            keys = save.keys;
            collectedIds = save.collectedIds != null ? new List<string>(save.collectedIds) : new List<string>();

            RefreshCollectibleReferences();
            RestoreCollectedState();
        }

        private void RefreshCollectibleReferences()
        {
            Collectibles.Coins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Coins"));
            Collectibles.Keys = new List<GameObject>(GameObject.FindGameObjectsWithTag("Keys"));
        }

        private void RestoreCollectedState()
        {
            RestoreMatching(Collectibles.Coins);
            RestoreMatching(Collectibles.Keys);
        }

        //Despawn/respawn collectibles
        private void RestoreMatching(List<GameObject> objects)
        {
            foreach (GameObject go in objects)
            {
                CollectibleTriggerHandler handler = go.GetComponent<CollectibleTriggerHandler>();
                go.SetActive(!IsCollected(handler.Id));
            }
        }
    }
}