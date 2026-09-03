using System;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Snapshot of player input, collected each Update by PlayerInputHandler
    /// and consumed each FixedUpdate by PlayerCharacterController.
    /// </summary>
    public struct PlayerInputs
    {
        public float MoveInput;
        public Vector3 CameraForward;   // pre-flattened to the horizontal plane
        public Vector3 CameraRight;     // pre-flattened to the horizontal plane
        public float ClimbInput;
        public bool DrawInput;
    }

    [Flags]
    public enum PlayerTrigger
    {
        None  = 0,
        Jump  = 1 << 0,
        Dash  = 1 << 1,
        Interact = 1 << 2,

        RotateCameraLeft = 1 << 3,
        RotateCameraRight = 1 << 4,
    }

    static class CommandUtils
    {
        public static bool IsUp(PlayerTrigger flags, PlayerTrigger trigger)
        {
            return (trigger & flags) == trigger;
        }

        public static void Off(ref PlayerTrigger flags, PlayerTrigger trigger)
        {
             flags &= ~trigger;
        }

        public static void On(ref PlayerTrigger flags, PlayerTrigger trigger)
        {
            flags |= trigger;
        }

        public static void Clear(ref this PlayerTrigger flags)
        {
            flags = PlayerTrigger.None;
        }
    }
}
