using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooting : TurretRotationState
{
    private readonly float _fireDelay;
    private readonly float _fireRotationDelay;

    private readonly LayerMask _shotObstructionLayers;
    private readonly float _attackDamage;

    private readonly int _burstCount;
    private readonly float _burstDelay;


    private float _fireDelayRemaining = 0f;
    private bool _isAttacking = false;

    private bool _canRotate = true; // Used for if we want the turret to pause its rotation when it fires.

    public TurretShooting(EnemyTurret brain, Transform rotTarget, float rotSpeed, float fireDelay, float fireRotationDelay, LayerMask shotObstructionLayers, float shotDamage, int bursts, float burstDelay) : base(brain, rotTarget, rotSpeed)
    {
        this._fireDelay = fireDelay;
        this._fireRotationDelay = fireRotationDelay;

        this._shotObstructionLayers = shotObstructionLayers;

        this._attackDamage = shotDamage;
        this._burstCount = bursts;
        this._burstDelay = burstDelay;
    }


    public override void Tick()
    {
        _fireDelayRemaining -= Time.deltaTime;

        if (_canRotate)
            base.Tick();

        // Handle Shooting.
        if (_brain.Target != null && !_isAttacking && _fireDelayRemaining <= 0f)
        {
            _brain.StartCoroutine(HandleShooting());
        }
    }


    IEnumerator HandleShooting()
    {
        _canRotate = false;
        _isAttacking = true;
        int burstsRemaining = _burstCount;

        while (burstsRemaining > 0)
        {
            Fire();
            yield return new WaitForSeconds(_burstDelay);
            burstsRemaining--;
        }

        _isAttacking = false;
        _fireDelayRemaining = _fireDelay;

        yield return new WaitForSeconds(_fireRotationDelay);
        _canRotate = true;
    }

    void Fire()
    {
        // Play Fire Effects.

        if (_brain.Target == null)
            return;

        if (Physics.Linecast(_rotationTarget.position, _brain.Target.position, _shotObstructionLayers))
            return;

        Debug.Log("Bang!");
        _brain.Target.GetComponent<HealthComponent>().TakeDamage(_attackDamage);
    }


    public override void OnEnter()
    {
        base.OnEnter();
    }
    public override void OnExit()
    {
        base.OnExit();

        _isAttacking = false;
        _canRotate = true;
        _fireDelayRemaining = 0f;
    }
}
