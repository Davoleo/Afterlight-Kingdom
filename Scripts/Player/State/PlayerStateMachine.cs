using Controllers;

namespace Player.State
{
    
    public class PlayerStateMachine
    {
        public readonly GroundedState GroundedState = new();
        public readonly DashingState DashingState = new();
        public readonly AirborneState AirborneState = new();
        public readonly ClimbingState ClimbingState = new();
        
        public PlayerState CurrentState { get; private set; }
        private readonly PlayerCharacterController _ctrl;

        public PlayerStateMachine(PlayerCharacterController ctrl)
        {
            _ctrl = ctrl;
            GroundedState.SetContext(ctrl);
            AirborneState.SetContext(ctrl);
            DashingState.SetContext(ctrl);
            ClimbingState.SetContext(ctrl);
            CurrentState = GroundedState;
        }
        
        public void TransitionToState(PlayerState next)
        {
            CurrentState.OnExit(next);
            CurrentState = next;
            CurrentState.OnEnter();
        }
    }
}