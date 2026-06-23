using Controllers;
using Core;
using UnityEngine;

namespace Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        public Vector3 lastCheckPoint = new Vector3(0, 2, 0);

        private GameObject _player;
        private PlayerCharacterController _playerController;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _playerController = _player.GetComponent<PlayerCharacterController>();

            var save = SaveManager.Load();
            if (save == null) return;
            lastCheckPoint = new Vector3(save.checkpointX, save.checkpointY, save.checkpointZ);
            Respawn();
        }

        public void Respawn()
        {
            _playerController.motor.SetPosition(lastCheckPoint);
        }
    }
}