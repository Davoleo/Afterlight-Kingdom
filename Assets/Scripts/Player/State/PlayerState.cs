using System;
using UnityEngine;

namespace Player.State
{
    // State pattern implementation
    public abstract class PlayerState
    {
        protected PlayerCharacterController Ctx;
        public static event Action OnJumped;
        public static event Action OnShoot;

        public void SetContext(PlayerCharacterController ctx)
        {
            this.Ctx = ctx;
        }

        protected static void InvokeJumpEvent()
        {
            OnJumped?.Invoke();
        }

        protected static void InvokeShootEvent()
        {
            OnShoot?.Invoke();
        }
        
        public virtual void OnEnter() {}
        public virtual void OnExit(PlayerState next) {}
        public abstract void UpdateVelocity(ref Vector3 vel, float dt);
        public virtual void BeforeUpdate(float dt) {}
    }
}