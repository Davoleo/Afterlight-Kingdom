using Core;
using UnityEngine;

namespace Controllers
{
    public class MenuActions : MonoBehaviour
    {
        public void RestartFromCheckpoint()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene("MainScene");
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