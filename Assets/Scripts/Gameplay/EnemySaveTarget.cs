using System.Collections.Generic;
using Core;
using Enemies;
using UnityEngine;

namespace Gameplay
{
    public class EnemySaveTarget : MonoBehaviour
    {
        private static readonly List<EnemySaveTarget> RegisteredEnemies = new List<EnemySaveTarget>(); 
        
        [SerializeField] private string enemyId;

        private CharacterController _characterController;
        private EnemyHealth _enemyHealth;
        // Enemy params to use for respawn
        private Vector3 _spawnPosition; 
        private Quaternion _spawnRotation; 

        public string EnemyId => enemyId;
        public bool IsAlive => !_enemyHealth.IsDead;

        public static IReadOnlyList<EnemySaveTarget> Enemies => RegisteredEnemies; 

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearRegisteredEnemies()
        {
            RegisteredEnemies.Clear(); 
        }
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying || !gameObject.scene.IsValid() || UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            EnemySaveTarget[] enemies = FindObjectsByType<EnemySaveTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            HashSet<string> usedIds = new HashSet<string>();
            bool hasDuplicateId = false;

            foreach (EnemySaveTarget enemy in enemies)
            {
                if (enemy == this || enemy.gameObject.scene != gameObject.scene)
                    continue;

                if (!string.IsNullOrEmpty(enemy.enemyId))
                    usedIds.Add(enemy.enemyId);

                if (!string.IsNullOrEmpty(enemyId) && enemy.enemyId == enemyId)
                    hasDuplicateId = true;
            }

            if (!string.IsNullOrEmpty(enemyId) && !hasDuplicateId)
                return;

            int nextId = 1;
            string generatedId;

            do
            {
                generatedId = $"{gameObject.scene.name}_Enemy_{nextId:D3}";
                nextId++;
            }
            while (usedIds.Contains(generatedId));

            enemyId = generatedId;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        private void Start()
        {
            // Original enemy position
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;

            _characterController = GetComponent<CharacterController>();
            _enemyHealth = GetComponent<EnemyHealth>();

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