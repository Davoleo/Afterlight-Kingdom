using UnityEngine;

namespace Enemies
{
    public class EnemyActivationTrigger : MonoBehaviour
    {
        [SerializeField] private EnemyController enemy;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (enemy == null)
            {
                Debug.LogWarning("EnemyActivationTrigger: enemy non assegnato.");
                return;
            }

            enemy.Activate();
        }
    }
}