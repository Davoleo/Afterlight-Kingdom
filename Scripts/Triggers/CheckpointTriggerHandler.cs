using Core;
using Gameplay;
using HUD;
using Player;
using UnityEngine;

namespace Triggers
{
    public class CheckpointTriggerHandler : MonoBehaviour
    {
        private HealthManager _healthManager;
        private CheckpointManager _cpManager;
        private CollectiblesManager _collectiblesManager;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            _healthManager = player.GetComponent<HealthManager>();
            var gm = GameObject.FindGameObjectWithTag("GameManager");
            _cpManager = gm.GetComponent<CheckpointManager>();
            _collectiblesManager = gm.GetComponent<CollectiblesManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            _healthManager.Heal(HealthManager.MaxHealth);
            var offsetPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
            _cpManager.lastCheckPoint = offsetPos;
            // Save progress -> new checkpoint position and Collectibles
            SaveManager.Save(offsetPos, _collectiblesManager.collectedIds, _collectiblesManager.coins, _collectiblesManager.keys);
            _cpManager.ShowSavedMessage();
        }
    }
}