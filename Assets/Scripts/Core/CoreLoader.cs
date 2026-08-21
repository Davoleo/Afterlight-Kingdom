using System.Collections;
using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    // Owns the entire Core boot sequence end to end: resolve which level to load, load it,
    // activate it, position the player at the right checkpoint, then - and only then -
    // declare the game Playing. Doing this as one linear sequence (instead of CheckpointManager,
    // AbilityManager etc. each guessing independently in their own Start() whether the level is
    // ready) removes the Start()-order races that come from coordinating through static flags.
    public class CoreLoader : MonoBehaviour
    {
        private void Start()
        {
            GameStateManager.SetState(GameState.Loading);
            StartCoroutine(BootCore());
        }

        private IEnumerator BootCore()
        {
            // 1. Determine which level to load: if a save exists, use the level the last
            // checkpoint was recorded in, otherwise use the one chosen from the menu (new game)
            string levelName = GameSession.ResolveLevelToLoad();

            // If the level is already loaded (e.g. it was left open in the Editor's multi-scene
            // Hierarchy before pressing Play), don't load a second copy of it on top. Core itself
            // is explicitly excluded - it must never be mistaken for the resolved level name.
            Scene existingScene = SceneManager.GetSceneByName(levelName);
            bool alreadyLoaded = levelName != SceneNames.Core && existingScene.IsValid() && existingScene.isLoaded;

            if (!alreadyLoaded)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
                if (asyncLoad == null)
                {
                    Debug.LogError($"CoreLoader: couldn't load level '{levelName}' - is it added to Build Settings?");
                    yield break;
                }

                // 2. Wait until it has finished loading
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                existingScene = SceneManager.GetSceneByName(levelName);
            }

            // set the newly loaded level as the Active Scene
            // This ensures the level's lighting (skybox, fog) is applied
            // and that newly spawned objects end up in the level instead of in Core.
            SceneManager.SetActiveScene(existingScene);

            // 3. The level's colliders/geometry now genuinely exist - safe to position the player.
            GetComponent<CheckpointManager>().Respawn();

            GameStateManager.SetState(GameState.Playing);
        }
    }
}