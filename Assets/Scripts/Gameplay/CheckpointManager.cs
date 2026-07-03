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

                lastPlayerRotation = save.playerRotationY;

                //restore enemies
                EnemySaveManager.RestoreEnemyStates(save.enemyStates);
            }

            _playerController.StopExternalKnockback();

            //_playerController.RestoreRotationY(lastPlayerRotation);

            //restore player location
            _playerController.motor.SetPositionAndRotation(
                lastCheckPoint,
                Quaternion.Euler(0f, lastPlayerRotation, 0f)
            );
        }
    }
}