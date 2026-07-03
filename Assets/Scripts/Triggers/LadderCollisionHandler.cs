using Controllers;
using Player;
using Player.State;
using UnityEngine;

namespace Triggers
{
    public class LadderCollisionHandler : MonoBehaviour
    {

        private PlayerCharacterController _controller;
        private PlayerStateMachine _stateMachine;

        private void Start()
        {
            _controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacterController>();
            _stateMachine = _controller.StateMachine;
            
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (_stateMachine.CurrentState != _stateMachine.ClimbingState)
                {
                    Debug.Log(_stateMachine.CurrentState);

                    // TODO: update character rotation to match ladder climbing
                    _stateMachine.TransitionToState(_stateMachine.ClimbingState);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (_stateMachine.CurrentState == _stateMachine.ClimbingState)
                {
                    _stateMachine.TransitionToState(_controller.IsGrounded ?
                        _stateMachine.GroundedState : _stateMachine.AirborneState);
                }
            }
        }
    }
}
