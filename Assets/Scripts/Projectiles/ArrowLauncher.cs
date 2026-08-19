using System.Collections.Generic;
using UnityEngine;

namespace Projectiles
{
    public class ArrowLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, 0.5f);
        [SerializeField] private int maxArrowsInWorld = 3;

        private readonly Queue<GameObject> _activeArrows = new();
        private float _nextFireTime;

        public void Shoot(Vector3 direction)
        {
            PurgeDestroyedArrows();

            if (_activeArrows.Count >= maxArrowsInWorld)
                Destroy(_activeArrows.Dequeue());

            Vector3 spawnPos = transform.position + transform.rotation * spawnOffset;
            GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(direction));
            arrow.GetComponent<Arrow>().Launch(direction);
            _activeArrows.Enqueue(arrow);
        }

        private void PurgeDestroyedArrows()
        {
            int count = _activeArrows.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject arrow = _activeArrows.Dequeue();
                if (arrow) _activeArrows.Enqueue(arrow);
            }
        }

        public void ClearAllArrows()
        {
            foreach (GameObject arrow in _activeArrows)
            {
                Destroy(arrow);
            }

            _activeArrows.Clear();
        }
    }
}
