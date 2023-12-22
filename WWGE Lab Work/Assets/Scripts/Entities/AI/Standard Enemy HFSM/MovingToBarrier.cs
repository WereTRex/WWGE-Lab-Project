using System;
using UnityEngine.AI;
using UnityEngine;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class MovingToBarrier : State
    {
        public override string Name { get => "Moving to Barrier"; }


        private readonly NavMeshAgent _agent;
        private readonly Func<Transform> _initialTarget;

        public MovingToBarrier(NavMeshAgent agent, Func<Transform> _initialTarget)
        {
            this._agent = agent;
            this._initialTarget = _initialTarget;
        }


        public override void OnEnter()
        {
            base.OnEnter();
            //_agent.enabled = true;
        }
        public override void OnLogic()
        {
            base.OnLogic();

            _agent.SetDestination(_initialTarget().position);
        }
        public override void OnExit()
        {
            base.OnExit();
            //_agent.enabled = false;
        }
    }
}