using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAlert : TurretRotationState
{
    private readonly ParticleSystem _alertPS;

    
    public TurretAlert(EnemyTurret brain, Transform rotTarget, float rotSpeed, ParticleSystem alertParticleSystem) : base(brain, rotTarget, rotSpeed)
    {
        this._alertPS = alertParticleSystem;
    }


    /*public override void Tick()
    {
        base.Tick();
    }*/


    public override void OnEnter()
    {
        base.OnEnter();
        _alertPS.Play();
    }
    /*public override void OnExit()
    {
        base.OnExit();
    }*/
}
