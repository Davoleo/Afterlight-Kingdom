using Core;
using UnityEngine;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class LevelTransitionTrigger : MonoBehaviour
    {
        public SceneNames nextLevelName;
        public Vector3 spawnPosition;
        public float spawnRotation;

        private bool _triggered;

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !other.CompareTag("Player")) return;
            _triggered = true;
            
            LoadingScreen.Instance.StartCoroutine(
                SceneTransitions.GoToLevel(nextLevelName, spawnPosition, spawnRotation, gameObject.scene.name));
        }
    }
}
