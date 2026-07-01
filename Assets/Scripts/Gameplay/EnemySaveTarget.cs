using Core;
using Projectiles;
using UnityEngine;
using KinematicCharacterController;

namespace Gameplay
{
    public class EnemySaveTarget : MonoBehaviour
    {
        [SerializeField] private string enemyId;

        //enemies params to use for respawn and motor to move them
        private Vector3 _spawnPosition; 
        private Quaternion _spawnRotation; 
        private KinematicCharacterMotor _motor;

        public string EnemyId => enemyId;
        public bool IsAlive => gameObject.activeSelf;

        private void Awake()
        {
            //original enemies position
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            _motor = GetComponent<KinematicCharacterMotor>();
        }

        public void SetAlive(bool isAlive)
        {
            gameObject.SetActive(isAlive);
        }

        public void RestoreState(EnemySaveData enemyState)
        {
            ClearStuckArrows();

            gameObject.SetActive(enemyState.isAlive);

            _motor.SetPositionAndRotation(_spawnPosition, _spawnRotation);

            //broadcast message to all enemies component to apply respawn whenever is necessary
            if (enemyState.isAlive)
                BroadcastMessage("ResetEnemy", SendMessageOptions.DontRequireReceiver);
        }
        
        private void ClearStuckArrows()
        {
            Arrow[] stuckArrows = GetComponentsInChildren<Arrow>(true);

            foreach (Arrow arrow in stuckArrows)
            {
                Destroy(arrow.gameObject);
            }
        }
    }
}