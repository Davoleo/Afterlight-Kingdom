using System;
using System.Collections;
using Gameplay;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    // Owns every scene transition in the game end to end: shows the LoadingScreen,
    // drives the async loads, reports progress, activates the level, places the player
    // and restores saved state. Flows are started on LoadingScreen.Instance (a
    // persistent object) so they outlive the scene that launched them.
    public static class SceneTransitions
    {
        private static bool InProgress { get; set; }

        // Combined progress for the two-phase MainMenu -> Core -> level transition.
        // Unity caps AsyncOperation.progress at 0.9 until activation, so each raw
        // reading is normalized by 0.9 first. Phase 1 fills 0..0.5, phase 2 fills 0.5..1.
        private static float CombinedProgress(int phase, float rawProgress)
        {
            float norm = Mathf.Clamp01(rawProgress / 0.9f);
            return phase <= 1 ? 0.5f * norm : 0.5f + 0.5f * norm;
        }

        // ---- public flows -----------------------------------------------------

        public static IEnumerator EnterGame() => Guarded(EnterGameBody());

        public static IEnumerator GoToLevel(SceneNames next, Vector3 spawnPos, float spawnRot, string previousLevelName)
            => Guarded(GoToLevelBody(next, spawnPos, spawnRot, previousLevelName));

        // ---- re-entry + cleanup guard ---------------------------------------

        // Wraps a flow body: refuses to start a second concurrent transition, and
        // guarantees InProgress is cleared even if the body throws or is stopped.
        private static IEnumerator Guarded(IEnumerator body)
        {
            if (InProgress) yield break;
            InProgress = true;
            try
            {
                while (body.MoveNext())
                    yield return body.Current;
            }
            finally
            {
                InProgress = false;
            }
        }

        // ---- flow bodies ----------------------------------------------------

        private static IEnumerator EnterGameBody()
        {
            GameStateManager.SetState(GameState.Loading);
            yield return LoadingScreen.Instance.Show();

            // Phase 1: swap MainMenu out for Core.
            AsyncOperation coreOp = SceneManager.LoadSceneAsync(nameof(SceneNames.Core), LoadSceneMode.Single);
            while (coreOp is { isDone: false })
            {
                LoadingScreen.Instance.Report(CombinedProgress(1, coreOp.progress));
                yield return null;
            }

            // Phase 2: load the resolved level additively, then finish the boot.
            yield return LoadLevelAndFinish();

            yield return LoadingScreen.Instance.Hide();
        }

        // Resolve the level, additively load it (unless already loaded) reporting phase-2
        // progress (0.5..1), activate it, place the player, restore save, go Playing.
        private static IEnumerator LoadLevelAndFinish()
        {
            string level = GameSession.ResolveLevelToLoad();

            if (level == nameof(SceneNames.Core))
            {
                Debug.LogError("SceneTransitions: resolved level is 'Core' - aborting transition.");
                yield break;
            }

            Scene existing = SceneManager.GetSceneByName(level);
            bool alreadyLoaded = existing.IsValid() && existing.isLoaded;

            if (!alreadyLoaded)
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
                if (op == null)
                {
                    // It will happen, thank me later
                    Debug.LogError($"SceneTransitions: couldn't load level '{level}' - is it in Build Settings?");
                    yield break;
                }

                while (!op.isDone)
                {
                    LoadingScreen.Instance.Report(CombinedProgress(2, op.progress));
                    yield return null;
                }
            }

            LoadingScreen.Instance.Report(1f);

            // Post-load. The level's colliders, Coin/Key/Door objects etc. genuinely
            // exist now, so it's safe to make it active, restore saved state and place
            // the player. Doing this here removes Start()-order races.
            existing = SceneManager.GetSceneByName(level);
            SceneManager.SetActiveScene(existing);
            GameSession.SetCurrentLevel(Enum.Parse<SceneNames>(level));

            var gm = GameObject.FindGameObjectWithTag("GameManager");
            if (gm == null)
            {
                Debug.LogError("SceneTransitions: no GameManager in the Core scene - boot aborted.");
                yield break;
            }

            // Restore first so the collectibles/doors HUD settles while still covered by
            // the loading screen, rather than visibly ticking after it fades.
            SaveData save = SaveManager.HasSave ? SaveManager.Load() : null;

            if (save is not null) SaveManager.RestoreEverything(gm, save, true);
            else SaveManager.Save(gm);

            gm.GetComponent<CheckpointManager>().Respawn(save);

            GameStateManager.SetState(GameState.Playing);
        }

        private static IEnumerator GoToLevelBody(SceneNames next, Vector3 spawnPos, float spawnRot, string previousLevelName)
        {
            // Freeze the player (timeScale 0) and music and cover the swap.
            GameStateManager.SetState(GameState.Loading);
            yield return LoadingScreen.Instance.Show();

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(next.ToString(), LoadSceneMode.Additive);
            while (loadOp is { isDone: false })
            {
                LoadingScreen.Instance.Report(Mathf.Clamp01(loadOp.progress / 0.9f));
                yield return null;
            }
            LoadingScreen.Instance.Report(1f);

            Scene target = SceneManager.GetSceneByName(next.ToString());
            SceneManager.SetActiveScene(target);
            GameSession.SetCurrentLevel(next);

            var gm = GameObject.FindGameObjectWithTag("GameManager");
            var playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
            var cameraController = GameObject.FindWithTag("MainCamera").GetComponentInParent<PlayerCameraController>();
            if (SaveManager.HasSave)
            {
                SaveData save = SaveManager.Load();
                EnemySaveManager.RestoreEnemyStates(save.enemyStates, target.name);
            }

            playerController.motor.SetPosition(spawnPos);
            cameraController.SetRotationY(spawnRot);

            // Level changed: save even if the character is not inside a Checkpoint.
            var cpManager = gm.GetComponent<CheckpointManager>();
            cpManager.SetCheckpoint(spawnPos, spawnRot);
            SaveManager.Save(gm);

            yield return SceneManager.UnloadSceneAsync(previousLevelName);

            GameStateManager.SetState(GameState.Playing);
            yield return LoadingScreen.Instance.Hide();
        }
    }
}