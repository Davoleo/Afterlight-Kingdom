#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Core
{
    public static class PlayFromBootstrap
    {
        [MenuItem("Tools/Always Play From Bootstrap Scene")]
        private static void Toggle()
        {
            var current = EditorSceneManager.playModeStartScene;
            EditorSceneManager.playModeStartScene = current == null
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity")
                : null;
        }
        
    }
}
#endif