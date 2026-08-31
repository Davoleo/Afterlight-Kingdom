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

        public int Coins { get; private set; }
        public int Keys { get; private set; }

        [NonSerialized]
        public List<string> CollectedIds;

        private readonly Dictionary<CollectibleType, AudioClip> _sfx = new();

        private void Start()
        {
            _sfx[CollectibleType.Coin] = Resources.Load<AudioClip>("Sound/coin_pickup");
            _sfx[CollectibleType.Key] = Resources.Load<AudioClip>("Sound/key");
        }

        public AudioClip GetPickupSound(CollectibleType type) => _sfx[type];

        public void Collect(CollectibleType type, string id)
        {
            bool isNewCollectible = RegisterCollectedId(id);

            if (!isNewCollectible)
                return;

            switch (type)
            {
                case CollectibleType.Coin:
                    Coins++;
                    break;

                case CollectibleType.Key:
                    Keys++;
                    break;
            }
        }

        public bool RegisterCollectedId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            CollectedIds ??= new List<string>();

            if (CollectedIds.Contains(id))
                return false;

            CollectedIds.Add(id);
            return true;
        }

        public bool IsCollected(string id)
        {
            return CollectedIds != null && CollectedIds.Contains(id);
        }

        public int GetCount(CollectibleType type)
        {
            return type switch
            {
                CollectibleType.Coin => Coins,
                CollectibleType.Key => Keys,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public bool UseKey()
        {
            if (Keys <= 0)
                return false;

            Keys--;
            return true;
        }

        public void SpendCoins(int amount) => Coins -= amount;

        // Called explicitly by CoreLoader once the level has actually finished loading and
        // become the active scene - same reasoning as CheckpointManager.Respawn(): the level's
        // Coin/Key objects don't exist yet while this GameObject is still booting inside Core.
        public void RestoreFromSave(SaveData save)
        {
            Coins = save.coins;
            Keys = save.keys;
            CollectedIds = save.collectedIds != null ? new List<string>(save.collectedIds) : new List<string>();

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