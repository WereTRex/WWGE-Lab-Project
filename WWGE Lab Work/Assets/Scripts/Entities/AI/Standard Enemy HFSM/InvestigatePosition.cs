using System;
using UnityEngine.AI;
using UnityEngine;
using UnityHFSM;

public class InvestigatePosition : State
{
    public override string Name { get => "Investigating"; }


    private readonly NavMeshAgent _agent;
    private readonly Func<Vector3> _suspiciousPosition;

    public InvestigatePosition(NavMeshAgent agent, Func<Vector3> suspiciousPosition)
    {
        this._agent = agent;
        this._suspiciousPosition = suspiciousPosition;
    }


    public override void OnLogic()
    {
        base.OnLogic();

        _agent.SetDestination(_suspiciousPosition());
    }
}