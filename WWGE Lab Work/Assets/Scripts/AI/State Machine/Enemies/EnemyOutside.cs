using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyOutside : IState
{
    private readonly Enemy _brain;
    private readonly NavMeshAgent _agent;

    private const float TARGET_UPDATE_DELAY = 0.2f;
    private float _targetUpdateDelayRemaining;

    public EnemyOutside(Enemy brain, NavMeshAgent agent)
    {
        this._brain = brain;
        this._agent = agent;
    }


    public void Tick()
    {
        if (_brain.InitialTarget == null)
            return;

        if (_targetUpdateDelayRemaining > 0)
        {
            _targetUpdateDelayRemaining -= Time.deltaTime;
            return;
        }

        _agent.SetDestination(_brain.InitialTarget.position);
        _targetUpdateDelayRemaining = TARGET_UPDATE_DELAY;
    }

    public void OnEnter()
    {
        _agent.enabled = true;

        _targetUpdateDelayRemaining = 0f;
    }
    public void OnExit()
    {
        _agent.enabled = false;
    }
}
