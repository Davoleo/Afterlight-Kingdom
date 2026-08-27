using System;
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
            string levelToLoad = GameSession.ResolveLevelToLoad();

            if (levelToLoad == SceneNames.Core.ToString())
            {
                // Never load Core as if it were a level,
                // doing so would spawn a second Core scene with its own
                // CoreLoader, which would repeat this and recurse without end.
                Debug.LogError("CoreLoader: resolved level is 'Core' - refusing to load it as a level.");
                yield break;
            }

            // If the level is already loaded (e.g. it was left open in the Editor's multi-scene
            // Hierarchy before pressing Play), don't load a second copy of it on top.
            Scene existingScene = SceneManager.GetSceneByName(levelToLoad);
            bool alreadyLoaded = existingScene.IsValid() && existingScene.isLoaded;

            if (!alreadyLoaded)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Additive);
                if (asyncLoad == null)
                {
                    Debug.LogError($"CoreLoader: couldn't load level '{levelToLoad}' - is it added to Build Settings?");
                    yield break;
                }

                // 2. Wait until it has finished loading
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                existingScene = SceneManager.GetSceneByName(levelToLoad);
            }

            // set the newly loaded level as the Active Scene
            // This ensures the level's lighting (skybox, fog) is applied
            // and that newly spawned objects end up in the level instead of in Core.
            SceneManager.SetActiveScene(existingScene);
            GameSession.SetCurrentLevel(Enum.Parse<SceneNames>(levelToLoad));

            // 3. The level's colliders/geometry now genuinely exist - safe to position the player.
            GetComponent<CheckpointManager>().Respawn();

            // Same reasoning: the level's Coin/Key/Door objects don't exist until now either, so
            // restoring collectible/door state has to wait until this point too.
            if (SaveManager.HasSave)
            {
                SaveData save = SaveManager.Load();
                GetComponent<CollectiblesManager>().RestoreFromSave(save);
                GetComponent<DoorManager>().RestoreFromSave(save);
            }

            GameStateManager.SetState(GameState.Playing);
        }
    }
}