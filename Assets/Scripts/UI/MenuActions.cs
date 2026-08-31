using Core;
using Gameplay;
using Player;
using UnityEngine;

namespace UI
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
            LoadingScreen.Instance.StartCoroutine(SceneTransitions.EnterGame());
        }

        public void Continue()
        {
            LoadingScreen.Instance.StartCoroutine(SceneTransitions.EnterGame());
        }

        public void QuitGame()
        {
            SceneLoader.QuitGame();
        }
    }
}