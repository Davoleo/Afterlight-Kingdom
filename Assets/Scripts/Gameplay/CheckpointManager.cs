using Core;
using Player;
using UnityEngine;

namespace Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        public Vector3 lastCheckPoint = new Vector3(0f, 2f, 0f);
        public float lastPlayerRotation = 90f;

        private PlayerCharacterController _playerController;
        private PlayerCameraController _playerCameraController;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            _playerController = player.GetComponent<PlayerCharacterController>();
            var mainCamera =  GameObject.FindGameObjectWithTag("MainCamera");
            _playerCameraController =  mainCamera.GetComponentInParent<PlayerCameraController>();

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

                lastPlayerRotation = save.playerRotationY;

                //restore collectibles
                GetComponent<CollectiblesManager>().RestoreFromSave(save);

                //restore enemies
                EnemySaveManager.RestoreEnemyStates(save.enemyStates);
            }

            _playerController.StopExternalKnockback();

            //restore player location
            _playerController.motor.SetPositionAndRotation(
                lastCheckPoint,
                Quaternion.Euler(0f, lastPlayerRotation, 0f)
            );

            //restore rotation state
            _playerCameraController.RestoreRotationY(lastPlayerRotation);
        }
    }
}