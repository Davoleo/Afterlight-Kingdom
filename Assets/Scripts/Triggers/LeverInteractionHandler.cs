using System;
using Player;
using UnityEngine;

namespace Triggers
{
    public class LeverInteractionHandler : MonoBehaviour
    {
        private static readonly int FlickedHash = Animator.StringToHash("Flicked");

        private bool _playerInRange;
        private bool flicked;

        private PlayerCharacterController _player;
        private Animator _leverAnimator;

        public static event Action<bool> LeverStateChanged;

        public bool WasActivated() => flicked;

        private void Start()
        {

            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
            _leverAnimator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!_playerInRange) return;

            if (CommandUtils.IsUp(_player.triggers, PlayerTrigger.Interact))
            {
                flicked = !flicked;
                CommandUtils.Off(ref _player.triggers, PlayerTrigger.Interact);

                LeverStateChanged?.Invoke(flicked);
            }

            _leverAnimator.SetBool(FlickedHash, flicked);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _playerInRange = false;
        }
    }
}