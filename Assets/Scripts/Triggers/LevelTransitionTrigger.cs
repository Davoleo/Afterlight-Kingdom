using System.Collections;
using Core;
using Gameplay;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            StartCoroutine(TransitionToLevel());
        }

        private IEnumerator TransitionToLevel()
        {
            string previousLevelName = gameObject.scene.name;

            // 1. Load the next level additively and wait for it to finish
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextLevelName.ToString(), LoadSceneMode.Additive);
            while (loadOp is { isDone: false })
            {
                yield return null;
            }

            // 2. Make the new level the active scene (lighting, new spawns end up there, etc.)
            Scene targetScene = SceneManager.GetSceneByName(nextLevelName.ToString());
            SceneManager.SetActiveScene(targetScene);

            // 3. Move the player to the new level's spawn point
            GameObject gm = GameObject.FindGameObjectWithTag("GameManager");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            var playerController = player.GetComponent<PlayerCharacterController>();
            var cameraController = GameObject.FindWithTag("MainCamera").GetComponentInParent<PlayerCameraController>();

            playerController.motor.SetPosition(spawnPosition);
            cameraController.SetRotationY(spawnRotation);

            // 4. Register the spawn point as the new checkpoint and persist the save immediately,
            // instead of relying on the player physically re-entering a checkpoint trigger
            var cpManager = gm.GetComponent<CheckpointManager>();
            cpManager.SetCheckpoint(spawnPosition, spawnRotation);
            SaveManager.Save(gm);

            // 5. The previous level is no longer needed once the player is in the new one
            SceneManager.UnloadSceneAsync(previousLevelName);
        }
    }
}
