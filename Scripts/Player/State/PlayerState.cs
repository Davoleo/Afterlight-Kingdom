using Controllers;
using UnityEngine;

namespace Player.State
{
    // State pattern implementation
    public abstract class PlayerState
    {
        protected PlayerCharacterController Ctx;

        public void SetContext(PlayerCharacterController ctx)
        {
            this.Ctx = ctx;
        }
        
        public virtual void OnEnter(){}
        public virtual void OnExit(PlayerState next){}
        public abstract void UpdateVelocity(ref Vector3 vel, float dt);
        public virtual void BeforeUpdate(float dt) { }
    }
}