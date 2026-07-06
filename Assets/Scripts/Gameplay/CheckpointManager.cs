using Core;
using Player;
using UnityEngine;

namespace Gameplay
{
    public struct Checkpoint
    {
        public Checkpoint(Vector3 position, float rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public float Rotation { get;  }
    }

    public class CheckpointManager : MonoBehaviour
    {
        public Checkpoint LastCheckPoint = new(new Vector3(0f, 2f, 0f), 0f);
        private PlayerCharacterController _playerController;
        private PlayerCameraController _cameraController;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            _playerController = player.GetComponent<PlayerCharacterController>();
            _cameraController = Camera.main.GetComponentInParent<PlayerCameraController>();

            Respawn();
        }

        public void Respawn()
        {
            if (SaveManager.HasSave)
            {
                SaveData save = SaveManager.Load();

                LastCheckPoint = new Checkpoint(new Vector3(save.checkpointX, save.checkpointY, save.checkpointZ), save.cameraRotation);

                //restore enemies
                EnemySaveManager.RestoreEnemyStates(save.enemyStates);
            }

            _playerController.StopExternalKnockback();

            //restore player location
            _playerController.motor.SetPosition(LastCheckPoint.Position);
            _cameraController.SetRotationY(LastCheckPoint.Rotation);
        }

        //function to recover both checkpoint and also last camera rotation registered
        public void SetCheckpoint(Vector3 checkpointPosition)
        {
            LastCheckPoint = new Checkpoint(
                checkpointPosition,
                _cameraController.GetRotationY()
            );
        }

    }
}