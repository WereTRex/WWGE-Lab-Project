using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.States
{
    public class Grounded : IState
    {
        private StateMachine _stateMachine;
        public Grounded(Idle idleState, Walking walkingState, Running runningState)
        {
            #region Setup Sub States.
            _stateMachine = new StateMachine();

            
            #endregion
        }


        public void Tick() { }

        public void OnEnter() { }
        public void OnExit() { }
    }
}