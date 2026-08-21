using System;
using UnityEngine;

namespace Core
{
    // Single source of truth for whether the game is actively playing, paused, or the
    // player is dead. Anything that must stop (or reset) when the game isn't playing
    // subscribes here, instead of every system independently checking Time.timeScale.
    public static class GameStateManager
    {
        public static GameState Current { get; private set; } = GameState.Playing;

        public static event Action<GameState> StateChanged;

        // Fired once the player has just respawned (position/health restored), so systems
        // holding onto transient state - animations, in-progress bow visuals, etc. - can
        // reset themselves back to a clean baseline.
        public static event Action Respawned;

        public static void SetState(GameState newState)
        {
            if (Current == newState) return;

            Current = newState;
            Time.timeScale = newState == GameState.Playing ? 1f : 0f;
            StateChanged?.Invoke(newState);
        }

        public static void NotifyRespawned()
        {
            Respawned?.Invoke();
        }
    }
}
