using Core;
using Gameplay;
using UnityEngine;

namespace Controllers
{
    public class MenuActions : MonoBehaviour
    {
        private CheckpointManager _cpManager;
        private MenuManager _menuManager;

        private void Start()
        {
            if (!gameObject.CompareTag("GameManager")) return;
            _cpManager = gameObject.GetComponent<CheckpointManager>();
            _menuManager = gameObject.GetComponent<MenuManager>();
        }
        
        public void RestartFromCheckpoint()
        {
            _cpManager.Respawn();
            _menuManager.CloseAllMenus();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene(0);
        }

        public void NewGame()
        {
            SaveManager.Delete();
            SceneLoader.LoadScene("MainScene");
        }

        public void Continue()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene("MainScene");
        }

        public void QuitGame()
        {
            SceneLoader.QuitGame();
        }
    }
}