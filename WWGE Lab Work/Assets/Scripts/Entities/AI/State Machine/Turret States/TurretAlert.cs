using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A state for when a turret has seen the player, but is not close enough to shoot.</summary>
public class TurretAlert : TurretRotationState
{
    private readonly ParticleSystem _alertPS;
    private readonly float _minimumDuration; // The minimum time that we can be in this state, even if we immediately lose sight of the player.

    private bool _minimumDurationElapsed;
    public bool MinimumDurationElapsed => _minimumDurationElapsed;

    
    public TurretAlert(EnemyTurret brain, Transform rotTarget, float rotSpeed, float minimumAlertDuration, ParticleSystem alertParticleSystem) : base(brain, rotTarget, rotSpeed)
    {
        this._alertPS = alertParticleSystem;
        this._minimumDuration = minimumAlertDuration;
    }


    public override void OnEnter()
    {
        base.OnEnter();

        // Play the alert particle system.
        _alertPS.Play();
        Brain.StartCoroutine(WaitForMinimumDuration());
    }
    public override void OnExit() => base.OnExit();
    

    private IEnumerator WaitForMinimumDuration()
    {
        _minimumDurationElapsed = false;
        yield return new WaitForSeconds(_minimumDuration);
        _minimumDurationElapsed = true;
    }
}
