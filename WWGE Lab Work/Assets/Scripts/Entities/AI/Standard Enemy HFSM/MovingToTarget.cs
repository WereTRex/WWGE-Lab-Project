using System;
using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class MovingToTarget : State
    {
        public override string Name { get => "Moving to Target"; }


        private readonly Func<Transform> _target;
        private readonly NavMeshAgent _agent;


        public MovingToTarget(NavMeshAgent agent, Func<Transform> target)
        {
            this._agent = agent;
            this._target = target;
        }


        public override void OnLogic()
        {
            base.OnLogic();

            _agent.SetDestination(_target().position);
        }
    }
}