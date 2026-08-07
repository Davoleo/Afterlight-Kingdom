using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Gameplay
{
    public class EnemySaveTarget : MonoBehaviour
    {
        private static readonly List<EnemySaveTarget> RegisteredEnemies = new List<EnemySaveTarget>(); 
        
        [SerializeField] private string enemyId;

        private CharacterController _characterController;

        // Enemy params to use for respawn
        private Vector3 _spawnPosition; 
        private Quaternion _spawnRotation; 

        public string EnemyId => enemyId;
        public bool IsAlive => gameObject.activeSelf;

        public static IReadOnlyList<EnemySaveTarget> Enemies => RegisteredEnemies; 

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearRegisteredEnemies()
        {
            RegisteredEnemies.Clear(); 
        }

        private void Awake()
        {
            // Original enemy position
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;

            _characterController = GetComponent<CharacterController>();

            if (gameObject.CompareTag("Enemy") && !RegisteredEnemies.Contains(this))
                RegisteredEnemies.Add(this);
        }

        private void OnDestroy()
        {
            RegisteredEnemies.Remove(this); 
        }

        public void SetAlive(bool isAlive)
        {
            gameObject.SetActive(isAlive);
        }

        public void RestoreState(EnemySaveData enemyState)
        {

            gameObject.SetActive(enemyState.isAlive);

            SetPositionAndRotation(_spawnPosition, _spawnRotation);

            // Broadcast message to all enemy components to apply respawn whenever necessary
            if (enemyState.isAlive)
                BroadcastMessage("ResetEnemy", SendMessageOptions.DontRequireReceiver);
        }

        private void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (_characterController == null)
            {
                transform.SetPositionAndRotation(position, rotation);
                return;
            }

            bool wasEnabled = _characterController.enabled;

            _characterController.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _characterController.enabled = wasEnabled;
        }
    }
}