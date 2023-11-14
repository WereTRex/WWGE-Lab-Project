using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretDeactivated : IState
{
    private readonly Repairable _repairableComponent;

    
    public TurretDeactivated(Repairable repairableComponent)
    {
        this._repairableComponent = repairableComponent;
        repairableComponent.enabled = false;
    }


    public void Tick() { }

    public void OnEnter()
    {
        _repairableComponent.enabled = true;
    }
    public void OnExit()
    {
        _repairableComponent.enabled = false;
    }
}
