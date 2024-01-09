using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class AttackingTarget : State
    {
        public override string Name { get => "Attacking Target"; }

        private readonly MonoBehaviour _mono;
        private readonly EnemyAttack[] _enemyAttacks; // Note: Arrays are reference values, so changing the default changes this :P

        public float MaxAttackRange { get; private set; }


        private readonly Func<Transform> _target;
        private readonly NavMeshAgent _agent;
        private readonly float _rotationSpeed;

        private readonly float _stoppingDistance;
        private float _oldStoppingDistance;


        private Coroutine _attackCoroutine;
        private Coroutine _updateCachedValuesCoroutine;
        private const float CACHED_INFO_UPDATE_DELAY = 0.3f;


        public AttackingTarget(MonoBehaviour mono, EnemyAttack[] attacks, Func<Transform> target, NavMeshAgent agent, float rotationSpeed, float stoppingDistance)
        {
            this._mono = mono;
            this._enemyAttacks = attacks;

            this._target = target;

            this._agent = agent;
            this._rotationSpeed = rotationSpeed;
            this._stoppingDistance = stoppingDistance;
        }


        public override void Init()
        {
            base.Init();

            // Set the MaxAttackRange.
            MaxAttackRange = _enemyAttacks.GetMaximumRange();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            // Start updating cached information.
            _updateCachedValuesCoroutine = _mono.StartCoroutine(UpdateCachedAttackValues());

            // Set stopping distance.
            _oldStoppingDistance = _agent.stoppingDistance;
            _agent.stoppingDistance = _stoppingDistance;
        }
        public override void OnLogic()
        {
            // If we are currently attacking, stop here.
            if (_attackCoroutine != null)
                return;


            // Move and rotate towards the target (Only if we are not currently attacking).
            _agent.SetDestination(_target().position);
            RotateToTarget(_agent.transform, _target());


            // Attempt to make an attack.
            float distanceToTarget = Vector3.Distance(_agent.transform.position, _target().position);
            float angleToTarget = Vector3.Angle(_agent.transform.forward, (_target().position - _agent.transform.position).normalized);
            _attackCoroutine = _mono.StartCoroutine(Attack(distanceToTarget, angleToTarget));

            // Stop moving if we have an attack.
            if (_attackCoroutine != null)
                _agent.SetDestination(_agent.transform.position);
        }
        public override void OnExit()
        {
            base.OnExit();

            // Stop the Attack Update Coroutine.
            if (_updateCachedValuesCoroutine != null)
                _mono.StopCoroutine(_updateCachedValuesCoroutine);

            // Revert the stopping distances.
            _agent.stoppingDistance = _oldStoppingDistance;
            _agent.updateRotation = true;
        }

        // Prevent transitioning if we are in the middle of an attack (Unless the transition uses forceInstantly).
        protected override bool CanExit() => _attackCoroutine != null;


        private IEnumerator Attack(float distanceToTarget, float angleToTarget)
        {
            // Check for a valid attack.
            if (TryGetAttack(distanceToTarget, angleToTarget, out EnemyAttack attack))
            {
                // Start the attack & wait until it has completed (Note: This includes recovery time).
                yield return _mono.StartCoroutine(attack.MakeAttack());

                // Allow further attacks.
                _attackCoroutine = null;
            }
        }
        private bool TryGetAttack(float distanceToTarget, float angleToTarget, out EnemyAttack foundAttack)
        {
            // Get all attacks that are within range & are not on cooldown.
            List<EnemyAttack> attackList = _enemyAttacks.Where(attack => (distanceToTarget <= attack.GetAttackRange()) && (angleToTarget <= attack.GetAttackAngle() / 2f) && !attack.IsInCooldown).ToList();

            if (attackList.Count > 0)
            {
                // If we have any valid attacks, output a random one and return true.
                foundAttack = attackList[UnityEngine.Random.Range(0, attackList.Count)];
                return true;
            }
            else
            {
                // There were no valid attacks, return false.
                foundAttack = null;
                return false;
            }
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


        private IEnumerator UpdateCachedAttackValues()
        {
            while (true)
            {
                // Update the MaxAttackRange.
                MaxAttackRange = _enemyAttacks.GetMaximumRange();

                // Wait for the next cached info update time.
                yield return new WaitForSeconds(CACHED_INFO_UPDATE_DELAY);
            }
        }
    }
}