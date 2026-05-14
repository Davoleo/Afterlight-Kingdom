using UnityEngine;

namespace Player.State
{
    public class DashingState : PlayerState
    {
        private readonly float _dashSpeed    = 30f;
        private readonly float _dashDuration = 0.2f; // seconds
        private float  _dashDurationTimer = 0.2f;
        private Vector3 _dashDirection;
        public override void OnEnter()
        {
            _dashDirection = Ctx.ComputeMoveDirection();
            if (_dashDirection == Vector3.zero) _dashDirection = Ctx.motor.CharacterForward;
            _dashDurationTimer = _dashDuration;
        }

        public override void UpdateVelocity(ref Vector3 vel, float dt)
        {
            vel  = _dashDirection * _dashSpeed;
            vel.y = 0f;   // keep it horizontal
            _dashDurationTimer -= dt;
            if (_dashDurationTimer <= 0f)
                Ctx.StateMachine.TransitionToState(Ctx.IsGrounded
                    ? Ctx.StateMachine.GroundedState
                    : Ctx.StateMachine.AirborneState);
        }
    }
}