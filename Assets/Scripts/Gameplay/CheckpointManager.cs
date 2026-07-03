using Controllers;
using Core;
using UnityEngine;

namespace Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        [SerializeField] public Vector3 lastCheckPoint = new Vector3(0f, 2f, 0f); 
        [SerializeField] public float lastPlayerRotation = 90f;

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
            _playerController.RestoreRotationY(lastPlayerRotation);
        }
    }
}