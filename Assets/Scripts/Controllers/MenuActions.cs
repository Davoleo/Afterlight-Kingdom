using Core;
using Gameplay;
using Player;
using UnityEngine;

namespace Controllers
{
    public class MenuActions : MonoBehaviour
    {
        private CheckpointManager _cpManager;
        private MenuManager _menuManager;
        private HealthManager _healthManager;
        
        private void Start()
        {
            if (!gameObject.CompareTag("GameManager")) return;
            _cpManager = gameObject.GetComponent<CheckpointManager>();
            _menuManager = gameObject.GetComponent<MenuManager>();

            var player = GameObject.FindGameObjectWithTag("Player");
            _healthManager = player.GetComponent<HealthManager>();
        }
        
        public void RestartFromCheckpoint()
        {
            _cpManager.Respawn();
            _healthManager.ResetAfterRespawn();
            _menuManager.CloseAllMenus(); // resumes play (sets GameState back to Playing)
        }

        public void ReturnToMenu()
        {
            GameStateManager.SetState(GameState.Playing);
            SceneLoader.LoadScene(SceneNames.MainMenu);
        }

        public void NewGame()
        {
            SaveManager.Delete();
            SceneLoader.LoadScene(SceneNames.Core);
        }

        public void Continue()
        {
            GameStateManager.SetState(GameState.Playing);
            SceneLoader.LoadScene(SceneNames.Core);
        }

        public void QuitGame()
        {
            SceneLoader.QuitGame();
        }
    }
}