using Controllers;
using Gameplay;
using Player;
using UnityEngine;

namespace Triggers
{
    public class DoorTriggerHandler : MonoBehaviour
    {
        private CollectiblesManager _manager;
        private PlayerCharacterController _controller;

        private bool _isClose;

        private void Start()
        {
            _manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CollectiblesManager>();
            _controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
        }

        private void Update()
        {
            if (!_isClose) return;
            if (!CommandUtils.IsUp(_controller.commands, PlayerCommand.Interact)) return;

            CommandUtils.Off(ref _controller.commands, PlayerCommand.Interact);
            if (_manager.UseKey())
                gameObject.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
                _isClose = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
                _isClose = true;
        }
    }
}
