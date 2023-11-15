using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurretRotationState : IState
{
    protected readonly EnemyTurret _brain;
    protected readonly Transform _rotationTarget;
    protected readonly float _rotationSpeed;

    public bool FacingTargetDirection = false;
    protected Vector3 _targetDir;

    public TurretRotationState(EnemyTurret brain, Transform rotTarget, float rotSpeed)
    {
        this._brain = brain;

        this._rotationTarget = rotTarget;
        this._rotationSpeed = rotSpeed;
    }


    public virtual void Tick()
    {
        // Rotate to face target.
        if (_brain.Target != null)
            _targetDir = (_brain.Target.transform.position - _rotationTarget.position).normalized;

        Vector3 newDirection = Vector3.RotateTowards(_rotationTarget.forward, _targetDir, _rotationSpeed * Time.deltaTime, 0.0f);
        _rotationTarget.rotation = Quaternion.LookRotation(newDirection);

        //FacingTargetDirection = Mathf.Abs(_rotationTarget.rotation - Quaternion.LookRotation(newDirection)) < 15f;
        FacingTargetDirection = Vector3.Angle(_targetDir, _rotationTarget.forward) < 5f;
    }

    public virtual void OnEnter()
    {
        //_brain.InvokeRepeating(nameof(_brain.TryGetTarget), 0, _brain.DetectionCheckDelay);
        _brain.StartDetection();
        _targetDir = _rotationTarget.transform.forward;
    }
    public virtual void OnExit()
    {
        _brain.StopDetection();
    }
}
