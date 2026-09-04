#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif
using UnityEngine.SceneManagement;

namespace Core
{
    public static class SceneLoader
    {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadScene(SceneNames sceneName)
        {
            SceneManager.LoadScene(sceneName.ToString());
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
            EditorApplication.isPlaying = false;
#else
            // compiled only in final build
            Application.Quit();
#endif
        }
    }
}