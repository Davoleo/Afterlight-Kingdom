using UnityEngine;

namespace Enemies
{
    public class EnemyActivationTrigger : MonoBehaviour
    {
        [SerializeField] private BaseEnemyController enemy;
        [SerializeField] private string playerTag = "Player";
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;
            enemy.Activate();
        }
    }
}