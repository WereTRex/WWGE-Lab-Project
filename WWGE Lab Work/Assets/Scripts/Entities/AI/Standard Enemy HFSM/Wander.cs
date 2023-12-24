using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class Wander : State
    {
        public override string Name { get => "Wander"; }


        private readonly NavMeshAgent _agent;
        private readonly float _stoppingDistance;

        private readonly float _maxWanderDistance;

        private readonly float _minIdleTime;
        private readonly float _maxIdleTime;
        private float _idleElapsedTime;
        private bool _idle;


        public Wander(NavMeshAgent agent, float stoppingDistance, float maxWanderDistance, float minIdleTime, float maxIdleTime)
        {
            this._agent = agent;
            this._stoppingDistance = stoppingDistance;

            this._maxWanderDistance = maxWanderDistance;

            this._minIdleTime = minIdleTime;
            this._maxIdleTime = maxIdleTime;
        }


        public override void OnEnter()
        {
            base.OnEnter();

            _idleElapsedTime = 0;
            _idle = false;
        }
        public override void OnLogic()
        {
            base.OnLogic();

            // If we are waiting at a point, we can stop here.
            if (_idleElapsedTime > Time.time)
                return;

            if (_idle)
            {
                // Set target point.
                SetWanderPosition();
            }
            else if (_agent.remainingDistance <= _stoppingDistance)
            {
                // We have reached the destination we were wandering to.
                _idleElapsedTime = Time.time + Random.Range(_minIdleTime, _maxIdleTime);
                _idle = true;
            }
        }


        private void SetWanderPosition()
        {
            // Get a random position on the NavMesh.
            Vector3 randomPosition = Random.insideUnitSphere * _maxWanderDistance;
            randomPosition += _agent.transform.position;

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, _maxWanderDistance, 1))
            {
                // Set the agent's target destination.
                _agent.SetDestination(hit.position);
            }
            else
            {
                // If we could not find a point, set the destination as the world origin.
                _agent.SetDestination(Vector3.zero);
            }
        }
    }
}