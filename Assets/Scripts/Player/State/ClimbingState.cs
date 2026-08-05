using UnityEngine;

namespace Player.State
{
    public class ClimbingState : PlayerState
    {

        private float _climbSpeed = 2.0f;

        public override void OnEnter()
        {
            //reset the player velocity to 0 to avoid
            Ctx.motor.BaseVelocity.x = 0;
            Ctx.motor.BaseVelocity.z = 0;
        }

        public override void UpdateVelocity(ref Vector3 vel, float dt)
        {
            
            var climbInput = Ctx.MoveInputs.ClimbInput.y;
            var moveInput = Ctx.MoveInputs.MoveInput.x;
            // TODO: implement jump during climbing state
            var jumpInput = CommandUtils.IsUp(Ctx.commands, PlayerCommand.Jump);

            //Player direction
            var direction = Ctx.transform.forward;

            if (Ctx.IsGrounded && climbInput > 0)
            {
                Ctx.motor.ForceUnground();
            }

            //Logic to handle ladder detachment when moving in the opposite direction of the ladder
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                // avoids unexpected detatches due to one of the directions not being 0 when moving in the opposite axis
                direction.x = Mathf.Abs(direction.x) < 0.01f ? 0 : direction.x;
                direction.z = Mathf.Abs(direction.z) < 0.01f ? 0 : direction.z;

                //Debug.Log("moveInput: " + moveInput);
                //player direction and input direction should be different (on both x and z axis)
                if (direction.z > 0f != moveInput > 0f || direction.x > 0f != moveInput > 0f)
                {
                    //Debug.Log("exiting climb because: z = " + direction.z + " x = " + direction.x + " moveInput =  " +  moveInput);
                    Ctx.StateMachine.TransitionToState(Ctx.IsGrounded
                        ? Ctx.StateMachine.GroundedState
                        : Ctx.StateMachine.AirborneState);
                }

            }
            //Debug.Log(vel.y);
            vel.y = climbInput *  _climbSpeed;
        }
    }
}