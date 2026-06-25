using UnityEngine.InputSystem;

namespace Core
{
    public static class InputUtils
    {
        public static bool IsGamepadActive()
        {
            var gamepad = Gamepad.current;
            var keyboard = Keyboard.current;
            if (gamepad == null) return false;
            if (keyboard == null) return true;
            return gamepad.lastUpdateTime >= keyboard.lastUpdateTime;
        }
    }
}
