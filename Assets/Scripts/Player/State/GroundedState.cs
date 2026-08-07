using System;
using UnityEngine;

namespace Player.State
{
    public class GroundedState : PlayerState
    {
        private readonly float _maxMoveSpeed = 5f;
        private readonly float _movementSharpness = 15f;
        
        public override void UpdateVelocity(ref Vector3 vel, float dt)
        {
            // Reorient current velocity to the slope normal so speed is preserved on ramps.
            vel = Ctx.motor.GetDirectionTangentToSurface(vel, Ctx.motor.GroundingStatus.GroundNormal) * vel.magnitude;

            if (CommandUtils.IsUp(Ctx.commands, PlayerCommand.Jump))
            {
                Ctx.motor.ForceUnground();  // tells KCC to stop snapping to ground this frame
                vel += (Ctx.jumpUpSpeed * Ctx.motor.CharacterUp)
                                   - Vector3.Project(vel, Ctx.motor.CharacterUp);
                InvokeJumpEvent();
                // State transition to Airborne happens in PostGroundingUpdate automatically.
                return;
            }

            Vector3 targetVelocity = Ctx.ComputeMoveDirection() * _maxMoveSpeed;

            // Exponential smoothing — frame-rate independent, same feel as Lerp but stable.
            vel = Vector3.Lerp(vel, targetVelocity,
                1f - Mathf.Exp(-_movementSharpness * dt));
        }
    }
}