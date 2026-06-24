using Core;
using Gameplay;
using Player;
using UnityEngine;

namespace Triggers
{
    public class CheckpointTriggerHandler : MonoBehaviour
    {
        private HealthManager _healthManager;
        private CheckpointManager _cpManager;
        private CollectiblesManager _collectiblesManager;
        private AbilityManager _abilityManager;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            _healthManager = player.GetComponent<HealthManager>();
            _abilityManager = player.GetComponent<AbilityManager>();

            GameObject gm = GameObject.FindGameObjectWithTag("GameManager");

            _cpManager = gm.GetComponent<CheckpointManager>();
            _collectiblesManager = gm.GetComponent<CollectiblesManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _healthManager.Heal(HealthManager.MaxHealth);

            Vector3 offsetPos = new Vector3(
                transform.position.x,
                transform.position.y + 2f,
                transform.position.z
            );

            _cpManager.lastCheckPoint = offsetPos;

            SaveManager.Save(
                offsetPos,
                _collectiblesManager.collectedIds,
                _collectiblesManager.coins,
                _collectiblesManager.keys,
                _abilityManager.unlockedAbilities
            );

            _cpManager.ShowSavedMessage();
        }
    }
}