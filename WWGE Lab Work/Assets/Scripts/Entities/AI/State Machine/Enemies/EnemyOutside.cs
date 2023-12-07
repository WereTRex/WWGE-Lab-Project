using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> A state for when an enemy is to move towards an initial target (Typically a Level Entrance Barrier).</summary>
public class EnemyOutside : IState
{
    private readonly Enemy _brain;
    private readonly NavMeshAgent _agent;

    private const float TARGET_UPDATE_DELAY = 0.2f;
    private float _targetUpdateDelayRemaining;

    public EnemyOutside(Enemy brain, NavMeshAgent agent)
    {
        // Set readonly values.
        this._brain = brain;
        this._agent = agent;
    }


    public void Tick()
    {
        // If there was no initial target, return until there is.
        if (_brain.InitialTarget == null)
            return;

        // If we shouldn't update the AI, then don't.
        if (_targetUpdateDelayRemaining > 0)
        {
            _targetUpdateDelayRemaining -= Time.deltaTime;
            return;
        }

        // Update the AI.
        _agent.SetDestination(_brain.InitialTarget.position);
        _targetUpdateDelayRemaining = TARGET_UPDATE_DELAY;
    }

    public void OnEnter()
    {
        // Enable the NavMeshAgent and reset the target update delay.
        _agent.enabled = true;
        _targetUpdateDelayRemaining = 0f;
    }
    public void OnExit()
    {
        // Disable the NavMeshAgent.
        _agent.enabled = false;
    }
}
