using UnityEngine;

namespace Player.State
{
    public class ClimbingState : PlayerState
    {

        private float _climbSpeed = 2.0f;
        
        public override void UpdateVelocity(ref Vector3 vel, float dt)
        {
            
            var climbInput = Ctx.MoveInputs.ClimbInput.y;
            var moveInput = Ctx.MoveInputs.MoveInput.x;
            // TODO: implement jump during climbing state
            var jumpInput = CommandUtils.IsUp(Ctx.commands, PlayerCommand.Jump);

            float xDirection = Ctx.transform.forward.x;
            float zDirection = Ctx.transform.forward.z;

            if (zDirection > 0f || xDirection > 0f)
            {
                if (moveInput > 0.01f || jumpInput)
                {
                    Ctx.StateMachine.TransitionToState(Ctx.IsGrounded
                        ? Ctx.StateMachine.GroundedState
                        : Ctx.StateMachine.AirborneState);
                }
            }
            else if (zDirection < 0f || xDirection < 0f)
            {
                if (moveInput < -0.01f || jumpInput)
                {
                    Ctx.StateMachine.TransitionToState(Ctx.IsGrounded
                        ? Ctx.StateMachine.GroundedState
                        : Ctx.StateMachine.AirborneState);
                }
            }
            
            vel.y = climbInput *  _climbSpeed;
        }
    }
}