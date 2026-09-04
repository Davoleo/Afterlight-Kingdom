using System;
using UnityEngine;

namespace Core
{
    // Floating options UI shown over whatever scene is active - the MainMenu, or the
    // paused Core scene. Follows the LoadingScreen pattern: a prefab under
    // Resources/OptionsMenu instantiated on demand. Only one can be open at a time.
    public static class OptionsMenu
    {
        private const string PrefabName = "OptionsMenu";

        private static GameObject _instance;
        private static Action _onClosed;

        // Unity's != treats a destroyed object as null, so this also
        // reports false if the instance was torn down by a scene change.
        public static bool IsOpen => _instance != null;

        public static GameObject Open(Action onClosed = null)
        {
            if (IsOpen) return _instance;

            var prefab = Resources.Load<GameObject>(PrefabName);
            if (prefab == null)
            {
                Debug.LogError($"OptionsMenu: no prefab found at Resources/{PrefabName}.");
                return null;
            }

            _onClosed = onClosed;
            _instance = UnityEngine.Object.Instantiate(prefab);
            _instance.name = PrefabName;
            return _instance;
        }

        public static void Close()
        {
            if (!IsOpen) return;

            UnityEngine.Object.Destroy(_instance);
            _instance = null;

            var callback = _onClosed;
            _onClosed = null;
            callback?.Invoke();
        }
    }
}
