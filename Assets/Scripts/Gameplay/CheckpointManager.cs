using Controllers;
using Core;
using UnityEngine;

namespace Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        public Vector3 lastCheckPoint = new Vector3(36f, 29f, 32f);
        public Vector3 lastPlayerRotation = new Vector3(0f, 0f, 0f);

        private PlayerCharacterController _playerController;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            _playerController = player.GetComponent<PlayerCharacterController>();

            Respawn();
        }

        public void Respawn()
        {
            if (SaveManager.HasSave)
            {
                SaveData save = SaveManager.Load();

                lastCheckPoint = new Vector3(
                    save.checkpointX,
                    save.checkpointY,
                    save.checkpointZ
                );

                lastPlayerRotation = new Vector3(
                    save.playerRotationX,
                    save.playerRotationY,
                    save.playerRotationZ
                );
                //restore rotation state
                _playerController.RestoreRotationY(save.playerRotationY);

            }

            _playerController.StopExternalKnockback();


            //restore player location
            _playerController.motor.SetPositionAndRotation(
                lastCheckPoint,
                Quaternion.Euler(lastPlayerRotation)
            );

        }
    }
}