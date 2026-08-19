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

            var charFacing = Ctx.transform.forward;
            if (Vector3.Dot(charFacing, Ctx.CurrentLadderNormal) > -1)
            {
                Ctx.motor.RotateCharacter(Quaternion.LookRotation(-Ctx.CurrentLadderNormal, Vector3.up));
            }

        }

        public override void UpdateVelocity(ref Vector3 vel, float dt)
        {
            var climbInput = Ctx.PlayerInputs.ClimbInput.y;
            var moveVector = Ctx.ComputeMoveDirection();

            if (CommandUtils.IsUp(Ctx.triggers, PlayerTrigger.Jump))
            {
                vel += Ctx.climbJumpStrength * (Ctx.motor.CharacterUp + Ctx.CurrentLadderNormal);
                InvokeJumpEvent();
                ExitState();
                return;
            }

            if (Ctx.IsGrounded && climbInput > 0)
            {
                Ctx.motor.ForceUnground();
            }

            //Logic to handle ladder detachment when moving in the opposite direction of the ladder
            if (Vector3.Dot(moveVector, Ctx.CurrentLadderNormal) > 0f)
            {
                //Debug.Log("exiting climb because: z = " + direction.z + " x = " + direction.x + " moveInput =  " +  moveVector);
                ExitState();
            }
            //Debug.Log(vel.y);
            vel.y = climbInput *  _climbSpeed;
        }

        private void ExitState()
        {
            Ctx.StateMachine.TransitionToState(Ctx.IsGrounded
                ? Ctx.StateMachine.GroundedState
                : Ctx.StateMachine.AirborneState);
        }
    }
}