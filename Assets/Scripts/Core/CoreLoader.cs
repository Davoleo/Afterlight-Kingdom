using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class CoreLoader : MonoBehaviour
    {
        // Raised once the level scene has finished loading and become the active scene.
        // Anything that needs level content (colliders, spawn points, etc.) to exist
        // before acting - e.g. CheckpointManager positioning the player - should wait for this
        // instead of assuming the level is ready by the time its own Start() runs.
        public static event Action LevelLoaded;
        public static bool IsLevelLoaded { get; private set; }

        private void Start()
        {
            IsLevelLoaded = false;

            // As soon as the Core scene is ready, load the chosen level additively
            StartCoroutine(LoadLevelAdditive());
        }

        private IEnumerator LoadLevelAdditive()
        {
            // 1. Determine which level to load: if a save exists, use the level the last
            // checkpoint was recorded in, otherwise use the one chosen from the menu (new game)
            string levelName = GameSession.ResolveLevelToLoad();

            // If the level is already loaded (e.g. it was left open in the Editor's multi-scene
            // Hierarchy before pressing Play), don't load a second copy of it on top.
            Scene existingScene = SceneManager.GetSceneByName(levelName);
            if (existingScene.IsValid() && existingScene.isLoaded)
            {
                SceneManager.SetActiveScene(existingScene);
                IsLevelLoaded = true;
                LevelLoaded?.Invoke();
                yield break;
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);

            // 2. Wait until it has finished loading
            while (asyncLoad is { isDone: false })
            {
                yield return null;
            }

            // set the newly loaded level as the Active Scene
            // This ensures the level's lighting (skybox, fog) is applied
            // and that newly spawned objects end up in the level instead of in Core.
            Scene targetScene = SceneManager.GetSceneByName(levelName);
            SceneManager.SetActiveScene(targetScene);

            IsLevelLoaded = true;
            LevelLoaded?.Invoke();
        }
    }
}