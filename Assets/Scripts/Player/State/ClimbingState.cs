using UnityEngine;

namespace Player.State
{
    public class ClimbingState : PlayerState
    {

        private float _climbSpeed = 2.0f;

        public override void OnEnter()
        {
            //reset the player velocity to 0 to avoid going through the ladder and colliding with weird boxes
            Ctx.motor.BaseVelocity.x = 0;
            Ctx.motor.BaseVelocity.z = 0;
        }

        public override void UpdateVelocity(ref Vector3 vel, float dt)
        {
            var climbInput = Ctx.MoveInputs.ClimbInput.y;
            var moveVector = Ctx.ComputeMoveDirection();
            // TODO: implement jump during climbing state
            var jumpInput = CommandUtils.IsUp(Ctx.commands, PlayerCommand.Jump);

            if (Ctx.IsGrounded && climbInput > 0)
            {
                Ctx.motor.ForceUnground();
            }

            //Logic to handle ladder detachment when moving in the opposite direction of the ladder
            if (Vector3.Dot(moveVector, Ctx.CurrentLadderNormal) > 0f)
            {
                //Debug.Log("exiting climb because: z = " + direction.z + " x = " + direction.x + " moveInput =  " +  moveVector);
                Ctx.StateMachine.TransitionToState(Ctx.IsGrounded
                    ? Ctx.StateMachine.GroundedState
                    : Ctx.StateMachine.AirborneState);
            }
            //Debug.Log(vel.y);
            vel.y = climbInput *  _climbSpeed;
        }
    }
}