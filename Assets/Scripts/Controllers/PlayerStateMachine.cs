using UnityEngine;

namespace Controllers
{
    public enum CharacterState
    {
        Grounded,
        Airborne,
        Dashing,    // not yet implemented — read HandleDashVelocity for step-by-step guidance
        Climbing,
    }
    
    public class PlayerStateMachine
    {
        public CharacterState CurrentState { get; private set; }
        private readonly PlayerCharacterController _ctrl;

        public PlayerStateMachine(PlayerCharacterController ctrl, CharacterState initialState)
        {
            _ctrl = ctrl;
            TransitionToState(initialState);
        }
        
        public void TransitionToState(CharacterState next)
        {
            OnStateExit(CurrentState, next);
            CurrentState = next;
            OnStateEnter(next);
        }

        private void OnStateEnter(CharacterState state)
        {
            switch (state)
            {
                case CharacterState.Grounded:
                    break;

                case CharacterState.Airborne:
                    break;

                case CharacterState.Dashing:
                    _ctrl.dashDirection = _ctrl.ComputeMoveDirection();
                    if (_ctrl.dashDirection == Vector3.zero) _ctrl.dashDirection = _ctrl.motor.CharacterForward;
                    _ctrl.dashDurationTimer = _ctrl.dashDuration;
                    break;
                case CharacterState.Climbing:
                    break;
            }
        }

        private void OnStateExit(CharacterState from, CharacterState to)
        {
            switch (from)
            {
                case CharacterState.Grounded:
                    break;

                case CharacterState.Airborne:
                    break;

                case CharacterState.Dashing:
                    // TODO: set character velocity to zero if entering Airborne state
                    break;
                case CharacterState.Climbing:
                    break;
            }
        }
    }
}