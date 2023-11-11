using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAlert : TurretRotationState
{
    private readonly ParticleSystem _alertPS;
    private readonly float _minimumDuration;

    private bool _minimumDurationElapsed;
    public bool MinimumDurationElapsed => _minimumDurationElapsed;

    
    public TurretAlert(EnemyTurret brain, Transform rotTarget, float rotSpeed, float minimumAlertDuration, ParticleSystem alertParticleSystem) : base(brain, rotTarget, rotSpeed)
    {
        this._alertPS = alertParticleSystem;
        this._minimumDuration = minimumAlertDuration;
    }


    /*public override void Tick()
    {
        base.Tick();
    }*/


    public override void OnEnter()
    {
        base.OnEnter();

        _alertPS.Play();
        _brain.StartCoroutine(WaitForMinimumDuration());
    }
    /*public override void OnExit()
    {
        base.OnExit();
    }*/


    private IEnumerator WaitForMinimumDuration()
    {
        _minimumDurationElapsed = false;
        yield return new WaitForSeconds(_minimumDuration);
        _minimumDurationElapsed = true;
    }
}
