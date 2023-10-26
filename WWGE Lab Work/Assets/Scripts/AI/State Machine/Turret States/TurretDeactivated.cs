using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretDeactivated : IState
{
    private readonly HealthComponent _healthComponent;
    private readonly float _deactivationDuration;
    private float _deactivationDurationRemaining;

    public bool DeactivationTimeElapsed { get => _deactivationDurationRemaining <= 0; }
    
    public TurretDeactivated(HealthComponent healthComponent, float deactivationDuration)
    {
        this._healthComponent = healthComponent;
        this._deactivationDuration = deactivationDuration;
    }


    public void Tick()
    {
        _deactivationDurationRemaining -= Time.deltaTime;
    }

    public void OnEnter()
    {
        _deactivationDurationRemaining = _deactivationDuration;
    }
    public void OnExit()
    {
        _healthComponent.ResetHealth();
    }
}
