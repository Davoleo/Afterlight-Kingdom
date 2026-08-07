using System;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Snapshot of player input, collected each Update by PlayerInputHandler
    /// and consumed each FixedUpdate by PlayerCharacterController.
    /// </summary>
    public struct MovementInputs
    {
        public Vector2 MoveInput;
        public Vector3 CameraForward;   // pre-flattened to the horizontal plane
        public Vector3 CameraRight;     // pre-flattened to the horizontal plane
        public Vector2 ClimbInput;
    }

    [Flags]
    public enum PlayerCommand
    {
        None  = 0,
        Jump  = 1 << 0,
        Dash  = 1 << 1,
        Shoot = 1 << 2,
        Interact = 1 << 3,

        RotateCameraLeft = 1 << 4,
        RotateCameraRight = 1 << 5,
    }

    static class CommandUtils
    {
        public static bool IsUp(PlayerCommand flags, PlayerCommand command)
        {
            return (command & flags) == command;
        }

        public static void Off(ref PlayerCommand flags, PlayerCommand command)
        {
             flags &= ~command;
        }

        public static void On(ref PlayerCommand flags, PlayerCommand command)
        {
            flags |= command;
        }

        public static void Clear(ref this PlayerCommand flags)
        {
            flags = PlayerCommand.None;
        }
    }
}
