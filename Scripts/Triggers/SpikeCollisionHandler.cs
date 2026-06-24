using Player;
using UnityEngine;

namespace Triggers
{
    public class SpikeCollisionHandler : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.gameObject.GetComponent<HealthManager>().TakeDamage(1);
            }
        }
    }
}
