using System.Linq;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UIEditor
{
    // Swaps plain UnityEngine.UI.Button components for MenuButton, keeping every other
    // serialized value (transition colours, navigation, OnClick) intact by only
    // rewriting m_Script - the serialized layout is compatible.
    public static class MenuButtonConverter
    {
        [MenuItem("Tools/UI/Convert Buttons To MenuButton")]
        private static void Convert()
        {
            Button[] targets = Selection.gameObjects.Length > 0
                ? Selection.gameObjects
                    .SelectMany(go => go.GetComponentsInChildren<Button>(true))
                    .Distinct()
                    .Where(b => b.GetType() == typeof(Button))
                    .ToArray()
                : AllInOpenScenesWithConfirm();

            if (targets == null) return;
            if (targets.Length == 0)
            {
                Debug.Log("MenuButtonConverter: no plain Button components in scope.");
                return;
            }

            MonoScript script = AssetDatabase.FindAssets("t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                .FirstOrDefault(ms => ms != null && ms.GetClass() == typeof(MenuButton));

            if (script == null)
            {
                Debug.LogError("MenuButtonConverter: couldn't locate the MenuButton MonoScript asset.");
                return;
            }

            int converted = 0;
            foreach (Button button in targets)
            {
                var so = new SerializedObject(button);
                so.FindProperty("m_Script").objectReferenceValue = script;
                so.ApplyModifiedProperties();

                EditorUtility.SetDirty(button);
                if (!EditorUtility.IsPersistent(button))
                    EditorSceneManager.MarkSceneDirty(button.gameObject.scene);
                converted++;
            }

            Debug.Log($"MenuButtonConverter: converted {converted} Button(s) to MenuButton. Save the scene/prefab to persist.");
        }

        private static Button[] AllInOpenScenesWithConfirm()
        {
            bool ok = EditorUtility.DisplayDialog(
                "Convert all buttons",
                "No selection. Convert every plain Button in all open scenes to MenuButton?",
                "Convert", "Cancel");

            if (!ok) return null;

            return Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(b => b.GetType() == typeof(Button))
                .ToArray();
        }
    }
}
