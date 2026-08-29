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
        public Checkpoint LastCheckPoint;

        private PlayerCharacterController _playerController;
        private PlayerCameraController _cameraController;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            LastCheckPoint = new Checkpoint(player.transform.position, 0);

            _playerController = player.GetComponent<PlayerCharacterController>();
            _cameraController = GameObject.FindWithTag("MainCamera").GetComponentInParent<PlayerCameraController>();

            // Respawn() is called explicitly by CoreLoader once the level has actually
            // finished loading - it needs the level's colliders/geometry to exist first.
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
        public void SetCheckpoint(Vector3 checkpointPosition, float rotation)
        {
            LastCheckPoint = new Checkpoint(
                checkpointPosition,
                rotation
            );
        }

    }
}