using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChasing : IState
{
    private const float AI_UPDATE_DELAY = 0.05f;

    private readonly StandardEnemy _brain;
    private readonly NavMeshAgent _agent;

    private Coroutine _aiUpdateCoroutine;

    
    public EnemyChasing(StandardEnemy brain, NavMeshAgent agent)
    {
        this._brain = brain;
        this._agent = agent;
    }


    public void Tick()
    {

    }

    public void OnEnter()
    {
        Debug.Log("Chasing");
        _agent.enabled = true;
        _aiUpdateCoroutine = _brain.StartCoroutine(UpdateAI());
    }
    public void OnExit()
    {
        _agent.enabled = false;
        _brain.StopCoroutine(_aiUpdateCoroutine);
    }


    private IEnumerator UpdateAI()
    {
        while (true)
        {
            _agent.SetDestination(_brain.GetTarget().position);

            yield return new WaitForSeconds(AI_UPDATE_DELAY);
        }
    }
}
