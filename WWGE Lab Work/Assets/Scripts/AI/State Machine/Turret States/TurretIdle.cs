using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretIdle : IState
{
    private readonly EnemyTurret _brain;


    public TurretIdle(EnemyTurret brain)
    {
        this._brain = brain;
    }
    

    public void Tick() { }


    public void OnEnter()
    {
        _brain.InvokeRepeating(nameof(_brain.TryGetTarget), 0, _brain.DetectionCheckDelay);
    }
    public void OnExit()
    {
        _brain.CancelInvoke();
    }
}
