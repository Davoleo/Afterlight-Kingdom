using Player;
using UnityEngine.InputSystem;

namespace Core
{

    public enum  ControlsType {
        KeyboardMouse,
        Gamepad
    }
    public static class InputUtils
    {
        private static ControlsType _activeControls;

        private static ControlsType GetActiveControls()
        {
            var gamepad = Gamepad.current;
            var keyboard = Keyboard.current;

            if (gamepad == null)
            {
                return _activeControls = ControlsType.KeyboardMouse;
            }

            if (keyboard == null) return _activeControls = ControlsType.Gamepad;

            return gamepad.lastUpdateTime >= keyboard.lastUpdateTime
                ? _activeControls = ControlsType.Gamepad
                : _activeControls = ControlsType.KeyboardMouse;
        }

        public static string ReplaceInputIdentifiers(string raw)
        {
            var processed = raw;

            foreach (var pair in PlayerInputHandler.IdentifierActionMap)
            {
                var action = pair.Value.action;
                var controls = GetActiveControls();

                processed = pair.Key switch
                {
                    "{Move}" => controls == ControlsType.KeyboardMouse
                        ? processed.Replace(pair.Key, "A/D")
                        : processed.Replace(pair.Key, "Left Stick or D-Pad Left/Right"),

                    "{Climb}" => controls == ControlsType.KeyboardMouse
                        ? processed.Replace(pair.Key, "W/S")
                        : processed.Replace(pair.Key, "Left Stick or D-Pad Up/Down"),

                    _ => processed.Replace(pair.Key,
                        action.GetBindingDisplayString(group: controls.ToString(),
                            options: InputBinding.DisplayStringOptions.DontUseShortDisplayNames))
                };
            }

            return processed;
        }
    }
}
