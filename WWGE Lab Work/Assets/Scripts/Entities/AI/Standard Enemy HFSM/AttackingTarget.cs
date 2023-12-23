using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class AttackingTarget : AttackingState
    {
        public override string Name { get => "Attacking Target"; }


        private readonly Func<Transform> _target;
        private readonly NavMeshAgent _agent;
        private readonly float _rotationSpeed;

        private readonly float _stoppingDistance;
        private float _oldStoppingDistance;


        public AttackingTarget(MonoBehaviour mono, EnemyAttack[] attacks, Func<Transform> target, NavMeshAgent agent, float rotationSpeed, float stoppingDistance) : base(mono, attacks)
        {
            this._target = target;
            this._agent = agent;
            this._rotationSpeed = rotationSpeed;
            this._stoppingDistance = stoppingDistance;
        }


        public override void OnEnter()
        {
            base.OnEnter();

            _oldStoppingDistance = _agent.stoppingDistance;
            _agent.stoppingDistance = _stoppingDistance;
        }
        public override void OnLogic()
        {
            base.OnLogic();
            
            // If we are currently attacking, stop here.
            if (AttackCoroutine != null)
                return;


            // Move and rotate towards the target (Only if we are not currently attacking).
            _agent.SetDestination(_target().position);
            RotateToTarget(_agent.transform, _target());


            // Attempt to make an attack.
            float distanceToTarget = Vector3.Distance(_agent.transform.position, _target().position);
            float angleToTarget = Vector3.Angle(_agent.transform.forward, (_target().position - _agent.transform.position).normalized);
            AttackCoroutine = Mono.StartCoroutine(Attack(distanceToTarget, angleToTarget));

            // Stop moving if we have an attack.
            if (AttackCoroutine != null)
                _agent.SetDestination(_agent.transform.position);
        }
        public override void OnExit()
        {
            base.OnExit();

            _agent.stoppingDistance = _oldStoppingDistance;
            _agent.updateRotation = true;
        }


        private void RotateToTarget(Transform rotationTarget, Transform target)
        {
            // Temp.
            if (_agent.remainingDistance < _agent.stoppingDistance)
            {
                _agent.updateRotation = false;

                /*Vector3 newDirection = Vector3.RotateTowards(rotationTarget.position, target.position, _rotationSpeed, 0.0f);
                rotationTarget.rotation = Quaternion.LookRotation(newDirection);*/

                Vector3 lookPos = target.position - rotationTarget.position;
                lookPos.y = 0f;

                Quaternion targetRotation = Quaternion.LookRotation(lookPos);
                _agent.transform.rotation = Quaternion.RotateTowards(rotationTarget.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
            else
            {
                _agent.updateRotation = true;
            }
        }
    }
}