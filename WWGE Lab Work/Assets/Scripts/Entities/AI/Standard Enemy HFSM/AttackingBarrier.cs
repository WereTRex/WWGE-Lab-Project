using System;
using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class AttackingBarrier : AttackingState
    {
        public override string Name { get => "Attacking Barrier"; }


        private readonly Func<Transform> _initialTarget;
        private readonly NavMeshAgent _agent;

        private readonly float _stoppingDistance;
        private float _oldStoppingDistance;

        public AttackingBarrier(MonoBehaviour mono, EnemyAttack[] attacks, Func<Transform> initialTarget, NavMeshAgent agent, float stoppingDistance) : base(mono, attacks)
        {
            this._initialTarget = initialTarget;
            this._agent = agent;
            this._stoppingDistance = stoppingDistance;
        }


        public override void OnEnter()
        {
            base.OnEnter();

            //_agent.enabled = true;
            _agent.SetDestination(_initialTarget().position);

            _oldStoppingDistance = _agent.stoppingDistance;
            _agent.stoppingDistance = _stoppingDistance;
        }
        public override void OnLogic()
        {
            base.OnLogic();

            // If we are currently attacking, stop here.
            if (AttackCoroutine != null)
                return;


            // Attempt to make an attack.
            float distanceToTarget = Vector3.Distance(Mono.transform.position, _initialTarget().position);
            AttackCoroutine = Mono.StartCoroutine(Attack(distanceToTarget));
        }
        public override void OnExit()
        {
            base.OnExit();

            _agent.stoppingDistance = _oldStoppingDistance;
            //_agent.enabled = false;
        }
    }
}