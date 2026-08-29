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
        private float _lastActivationTime = -Mathf.Infinity;
        private bool _ignoreUntilExit;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            _healthManager = player.GetComponent<HealthManager>();
            _gm = GameObject.FindGameObjectWithTag("GameManager");
            _cpManager = _gm.GetComponent<CheckpointManager>();
            //TODO: Maybe FindFirstObjectByType is not the best solution
            _checkPointHUD = FindFirstObjectByType<CheckPointHUD>();
            Collider checkpointCollider = GetComponent<Collider>();
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null && checkpointCollider.bounds.Intersects(playerCollider.bounds))
                _ignoreUntilExit = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // CoreLoader hasn't promoted the level to the active scene yet (can happen if the
            // player already overlaps this trigger while the level is still loading additively
            // on top of Core); saving now would record "Core" as the level name and corrupt
            // the save. Ignore the trigger; it'll fire again once the player moves
            // through it during real play.
            if (GameStateManager.Current != GameState.Playing) return;
            if (_ignoreUntilExit) return;
            if (Time.time < _lastActivationTime + activationCooldown) return;
            _lastActivationTime = Time.time;

            AudioManager.Instance.PlaySfx(enterSfx, 0.7f);

            _healthManager.Heal(HealthManager.MaxHealth);

            Vector3 offsetPos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);

            checkpointRingEffect?.Play();

            //set camera position and checkpoint position, then save
            _cpManager.SetCheckpoint(offsetPos, derivedCameraRotation);
            SaveManager.Save(_gm);

            _checkPointHUD.ShowSavedMessage();
        }
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _ignoreUntilExit = false;
        }
    }
}