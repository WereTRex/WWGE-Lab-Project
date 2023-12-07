using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A state for when a turret is at 0HP, waiting to be repaired.</summary>
public class TurretDeactivated : IState
{
    private readonly EnemyTurret _brain;
    private readonly Repairable _repairableComponent;

    
    public TurretDeactivated(EnemyTurret brain, Repairable repairableComponent)
    {
        // Set readonly variables.
        this._brain = brain;
        this._repairableComponent = repairableComponent;

        // Disable the Repairable Component (Ensures that it can't be immediately repaired).
        repairableComponent.enabled = false;
    }


    public void Tick() { }

    public void OnEnter()
    {
        _brain.Target = null;
        _repairableComponent.enabled = true;
    }
    public void OnExit() => _repairableComponent.enabled = false;
}
