using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A state for when the turret is actively shooting at a target.</summary>
public class TurretShooting : TurretRotationState
{
    private readonly Gun _gun;
    private readonly bool _pauseWhileShooting;


    private bool _canRotate = true; // Used for if we want the turret to pause its rotation when it fires.

    public TurretShooting(EnemyTurret brain, Transform rotTarget, float rotSpeed, Gun gun, bool pauseWhileShooting) : base(brain, rotTarget, rotSpeed)
    {
        // Set readonly variabes.
        this._gun = gun;
        this._pauseWhileShooting = pauseWhileShooting;
    }

    private void StartedReloading() => _canRotate = true;
    private void WeaponAmmoChanged(int arg1, int arg2) => _canRotate = false;


    public override void Tick()
    {
        if (_canRotate)
            base.Tick();
    }


    public override void OnEnter()
    {
        base.OnEnter();

        if (_pauseWhileShooting)
        {
            _gun.OnStartedReloading += StartedReloading;
            _gun.OnWeaponAmmoChanged += WeaponAmmoChanged;
        }

        _gun.StartAttacking();
    }
    public override void OnExit()
    {
        base.OnExit();

        if (_pauseWhileShooting)
        {
            _gun.OnStartedReloading -= StartedReloading;
            _gun.OnWeaponAmmoChanged -= WeaponAmmoChanged;
        }

        _gun.StopAttacking();
        _canRotate = true;
    }
}
