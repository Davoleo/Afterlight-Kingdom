using Gameplay;
using Player;
using UnityEngine;

namespace Triggers
{
    public class DoorTriggerHandler : MonoBehaviour
    {
        public string Id =>
            $"{transform.position.x}_{transform.position.y}_{transform.position.z}";

        private CollectiblesManager _collectiblesManager;
        private DoorManager _doorManager;
        private PlayerCharacterController _controller;

        private bool _isClose;

        private void Start()
        {
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
            _collectiblesManager = gameManager.GetComponent<CollectiblesManager>();
            _doorManager = gameManager.GetComponent<DoorManager>();
            _controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
        }

        private void Update()
        {
            if (!_isClose) return;
            if (!CommandUtils.IsUp(_controller.triggers, PlayerTrigger.Interact)) return;

            CommandUtils.Off(ref _controller.triggers, PlayerTrigger.Interact);
            
            if (!_collectiblesManager.UseKey()) return;
            
            _doorManager.RegisterOpenedDoor(Id);
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
