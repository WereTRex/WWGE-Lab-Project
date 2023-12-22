using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> A state for when an enemy is chasing a target.</summary>
public class EnemyChasing : IState
{
    private const float AI_UPDATE_DELAY = 0.05f;

    private readonly StandardEnemy _brain;
    private readonly NavMeshAgent _agent;

    private Coroutine _aiUpdateCoroutine;

    
    public EnemyChasing(StandardEnemy brain, NavMeshAgent agent)
    {
        // Set readonly values.
        this._brain = brain;
        this._agent = agent;
    }


    public void Tick() { }

    public void OnEnter()
    {
        // Enable the NavMeshAgent and start the AIUpdate Coroutine.
        _agent.enabled = true;
        _aiUpdateCoroutine = _brain.StartCoroutine(UpdateAI());
    }
    public void OnExit()
    {
        // Disable the NavMeshAgent and stop the AIUpdate Coroutine.
        _agent.enabled = false;
        _brain.StopCoroutine(_aiUpdateCoroutine);
    }


    // Update the AI every AI_UPDATE_DELAY seconds.
    private IEnumerator UpdateAI()
    {
        while (true)
        {
            // Set the agent's destination to the position of the current target.
            //_agent.SetDestination(_brain.GetTarget().position);

            yield return new WaitForSeconds(AI_UPDATE_DELAY);
        }
    }
}
