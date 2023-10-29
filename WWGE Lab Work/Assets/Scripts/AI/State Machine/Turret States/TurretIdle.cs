using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretIdle : IState
{
    private readonly EnemyTurret _brain;
    private readonly Transform _rotationTarget;
    private readonly Vector3[] _lookPoints;
    private readonly float _rotationSpeed;
    private readonly float _rotationPause;

    private int _rotationIndex;
    private float _rotationPauseRemaining;


    public TurretIdle(EnemyTurret brain, Transform rotationTarget, Vector3[] lookPoints, float rotationSpeed, float rotationPauseTime)
    {
        this._brain = brain;
        this._rotationTarget = rotationTarget;

        this._lookPoints = lookPoints;
        this._rotationSpeed = rotationSpeed;
        this._rotationPause = rotationPauseTime;
    }
    

    public void Tick()
    {
        // Decrement Pause Delay.
        if (_rotationPauseRemaining > 0f) {
            _rotationPauseRemaining -= Time.deltaTime;
            return;
        }

        // Look to next point.
        Vector3 targetDir = (_lookPoints[_rotationIndex] - _rotationTarget.position).normalized;
        Vector3 newDirection = Vector3.RotateTowards(_rotationTarget.forward, targetDir, _rotationSpeed * Time.deltaTime, 0.0f);
        _rotationTarget.rotation = Quaternion.LookRotation(newDirection);

        if (Vector3.Angle(targetDir, _rotationTarget.forward) < 5f) {
            _rotationPauseRemaining = _rotationPause;

            if (_rotationIndex < _lookPoints.Length - 1)
                _rotationIndex++;
            else
                _rotationIndex = 0;
        }
    }


    public void OnEnter()
    {
        _brain.InvokeRepeating(nameof(_brain.TryGetTarget), 0, _brain.DetectionCheckDelay);

        _rotationIndex = 0;
        _rotationPauseRemaining = 0;
    }
    public void OnExit()
    {
        _brain.CancelInvoke();
    }
}
