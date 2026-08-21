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
            Time.timeScale = 1f; //resume play after reload checkpoint
            _cpManager.Respawn();
            _healthManager.ResetAfterRespawn();
            _menuManager.CloseAllMenus();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene(SceneNames.MainMenu);
        }

        public void NewGame()
        {
            GameSession.LevelToLoad = SceneNames.Level1;
            SaveManager.Delete();
            SceneLoader.LoadScene(SceneNames.Core);
        }

        public void Continue()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene(SceneNames.Core);
        }

        public void QuitGame()
        {
            SceneLoader.QuitGame();
        }
    }
}