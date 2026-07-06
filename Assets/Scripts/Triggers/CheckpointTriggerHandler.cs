using Core;
using Gameplay;
using HUD;
using Player;
using UnityEngine;

namespace Triggers
{
    public class CheckpointTriggerHandler : MonoBehaviour
    {
        public float derivedCameraRotation;

        private GameObject _gm;
        private HealthManager _healthManager;
        private CheckpointManager _cpManager;
        private CheckPointHUD _checkPointHUD;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            _healthManager = player.GetComponent<HealthManager>();
            _gm = GameObject.FindGameObjectWithTag("GameManager");
            _cpManager = _gm.GetComponent<CheckpointManager>();
            //TODO: Maybe FindFirstObjectByType is not the best solution
            _checkPointHUD = FindFirstObjectByType<CheckPointHUD>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _healthManager.Heal(HealthManager.MaxHealth);

            Vector3 offsetPos = new Vector3(
                transform.position.x,
                transform.position.y + 2f,
                transform.position.z
            );
            //set camera position and checkpoint position, then save
            _cpManager.SetCheckpoint(offsetPos, derivedCameraRotation);
            SaveManager.Save(_gm);

            _checkPointHUD.ShowSavedMessage();
        }
    }
}