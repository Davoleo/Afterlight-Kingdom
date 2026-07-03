using System.Collections.Generic;
using Core;
using Projectiles;
using UnityEngine;
using KinematicCharacterController;

namespace Gameplay
{
    public class EnemySaveTarget : MonoBehaviour
    {
        private static readonly List<EnemySaveTarget> RegisteredEnemies = new List<EnemySaveTarget>(); // MODIFICA: lista dei nemici registrati senza usare FindObjectsOfType.

        [SerializeField] private string enemyId;

        //enemies params to use for respawn and motor to move them
        private Vector3 _spawnPosition; 
        private Quaternion _spawnRotation; 
        private KinematicCharacterMotor _motor;

        public string EnemyId => enemyId;
        public bool IsAlive => gameObject.activeSelf;

        public static IReadOnlyList<EnemySaveTarget> Enemies => RegisteredEnemies; // MODIFICA: permette all'EnemySaveManager di leggere i nemici registrati.

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearRegisteredEnemies()
        {
            RegisteredEnemies.Clear(); // MODIFICA: pulisce la lista quando viene ricaricato il gioco/la scena.
        }

        private void Awake()
        {
            //original enemies position
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            _motor = GetComponent<KinematicCharacterMotor>();

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