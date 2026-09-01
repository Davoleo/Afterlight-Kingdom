using UnityEngine;

namespace Player.State
{
    public class AirborneState : PlayerState
    {
        private readonly float _maxAirMoveSpeed = 5f;
        private readonly float _airAcceleration  = 5f;
        private readonly Vector3 _gravity = new Vector3(0f, -20f, 0f);

        public override void UpdateVelocity(ref Vector3 vel, float dt)
        {
            // Coyote jump: honour a jump press for a short grace period after walking off a ledge.
            if (CommandUtils.IsUp(Ctx.triggers, PlayerTrigger.Jump) && Ctx.CanCoyoteJump)
            {
                vel += (Ctx.jumpUpSpeed * Ctx.motor.CharacterUp)
                       - Vector3.Project(vel, Ctx.motor.CharacterUp);
                Ctx.ConsumeCoyote();
                InvokeJumpEvent();
            }

            // Partial air control: player can steer but not instantly change direction.
            if (Ctx.PlayerInputs.MoveInput.sqrMagnitude > 0.01f)
            {
                Vector3 targetHorizontal = Ctx.ComputeMoveDirection() * _maxAirMoveSpeed;
                Vector3 velocityDiff     = Vector3.ProjectOnPlane(targetHorizontal - vel, _gravity.normalized);
                vel += dt * _airAcceleration * velocityDiff;
            }

            vel += _gravity * dt;
        }
    }
}