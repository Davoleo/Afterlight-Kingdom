using Controllers;
using Core;
using UnityEngine;

namespace Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        public Vector3 lastCheckPoint = new Vector3(36f, 29f, 32f);

        private PlayerCharacterController _playerController;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            _playerController = player.GetComponent<PlayerCharacterController>();

            Respawn();
        }

        public void Respawn()
        {
            SaveData save = SaveManager.Load();

            if (SaveManager.HasSave)
            {
                lastCheckPoint = new Vector3(
                    save.checkpointX,
                    save.checkpointY,
                    save.checkpointZ
                );
            }

            _playerController.StopExternalKnockback();
            _playerController.motor.SetPosition(lastCheckPoint);
        }
    }
}