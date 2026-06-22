using UnityEngine.SceneManagement;

namespace Core
{
    public class SceneLoader
    {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        public static void QuitGame()
        {
            // preprocessor directive
#if UNITY_EDITOR
            // compiled only in editor
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // compiled only in final build
                    Application.Quit();
#endif
        }
    }
}