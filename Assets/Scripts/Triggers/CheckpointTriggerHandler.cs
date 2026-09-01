using System;
using Core;
using Gameplay;
using HUD;
using Player;
using Sound;
using UnityEngine;
using VisualEffects;

namespace Triggers
{
    public class CheckpointTriggerHandler : MonoBehaviour
    {
        public AudioClip enterSfx;

        public float derivedCameraRotation;

        [SerializeField] private CheckpointRingEffect checkpointRingEffect;
        [SerializeField] private float activationCooldown = 2f;

        private GameObject _gm;
        private HealthManager _healthManager;
        private CheckpointManager _cpManager;
        private CheckPointHUD _checkPointHUD;

        private const float MaxCooldown = 2f;
        private float _cooldown;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            _healthManager = player.GetComponent<HealthManager>();
            _gm = GameObject.FindGameObjectWithTag("GameManager");
            _cpManager = _gm.GetComponent<CheckpointManager>();
            //TODO: Maybe FindFirstObjectByType is not the best solution
            _checkPointHUD = FindFirstObjectByType<CheckPointHUD>();
            _cooldown = MaxCooldown;
            GameStateManager.Respawned += OnPlayerRespawn;
        }

        private void OnPlayerRespawn()
        {
            _cooldown = MaxCooldown;
        }

        private void Update() => _cooldown = Mathf.Max(_cooldown-Time.deltaTime, 0f);

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // CoreLoader hasn't promoted the level to the active scene yet (can happen if the
            // player already overlaps this trigger while the level is still loading additively
            // on top of Core); saving now would record "Core" as the level name and corrupt
            // the save. Ignore the trigger; it'll fire again once the player moves
            // through it during real play.
            if (GameStateManager.Current != GameState.Playing) return;
            //if (_ignoreUntilExit) return;
            // if (Time.time < _lastActivationTime + activationCooldown) return;
            // _lastActivationTime = Time.time;

            if (_cooldown > 0f) return;

            AudioManager.Instance.PlaySfx(enterSfx, 0.7f);

            _healthManager.Heal(HealthManager.MaxHealth);

            Vector3 offsetPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);

            checkpointRingEffect?.Play();

            //set camera position and checkpoint position, then save
            _cpManager.SetCheckpoint(offsetPos, derivedCameraRotation);
            SaveManager.Save(_gm);

            _checkPointHUD.ShowSavedMessage();
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _cooldown = MaxCooldown;
        }
    }
}