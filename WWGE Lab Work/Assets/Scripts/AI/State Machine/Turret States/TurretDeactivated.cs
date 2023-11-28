using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretDeactivated : IState
{
    private readonly EnemyTurret _brain;
    private readonly Repairable _repairableComponent;

    
    public TurretDeactivated(EnemyTurret brain, Repairable repairableComponent)
    {
        this._brain = brain;
        this._repairableComponent = repairableComponent;
        repairableComponent.enabled = false;
    }


    public void Tick() { }

    public void OnEnter()
    {
        _brain.Target = null;
        _repairableComponent.enabled = true;
    }
    public void OnExit()
    {
        _repairableComponent.enabled = false;
    }
}
