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
            var gm = GameObject.FindGameObjectWithTag("GameManager");
            if (gm == null) return;
            _cpManager = gm.GetComponent<CheckpointManager>();
            _menuManager = gm.GetComponent<MenuManager>();
        }

        public void RestartFromCheckpoint()
        {
            Time.timeScale = 1f;
            _cpManager.Respawn();
            _menuManager.CloseAllMenus();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene(0);
        }

        public void Play() => SceneLoader.LoadScene("MainScene");

        public void QuitGame() => SceneLoader.QuitGame();
    }
}
