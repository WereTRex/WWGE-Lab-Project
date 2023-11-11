using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretIdle : IState
{
    private readonly EnemyTurret _brain;
    private readonly Transform _rotationTarget;
    private readonly Vector3[] _lookPoints;
    
    private readonly float _maxRotationSpeed;
    private readonly float _rotationAcceleration;
    private readonly float _rotationDeceleration;

    private readonly float _rotationPause;


    private float _currentRotationSpeed;

    private int _rotationIndex;
    private float _rotationPauseRemaining;


    public TurretIdle(EnemyTurret brain, Transform rotationTarget, Vector3[] lookPoints, float rotationSpeed, float rotationAcceleration, float rotationDeceleration, float rotationPauseTime)
    {
        this._brain = brain;
        this._rotationTarget = rotationTarget;

        this._lookPoints = lookPoints;
        
        this._maxRotationSpeed = rotationSpeed;
        this._rotationAcceleration = rotationAcceleration;
        this._rotationDeceleration = rotationDeceleration;
        
        this._rotationPause = rotationPauseTime;
    }


    private float _foundTargetBuffer = 2f;
    bool _displayDebug = true;
    bool _debugAngles = true;
    public void Tick()
    {
        // Decrement Pause Delay.
        if (_rotationPauseRemaining > 0f) {
            _rotationPauseRemaining -= Time.deltaTime;
            return;
        }

        // Calculate the direction & rotation to the target.
        Vector3 targetDir = (_lookPoints[_rotationIndex] - _rotationTarget.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);


        // Distance to the target (In radians).
        float distanceRemaining = Quaternion.Angle(_rotationTarget.rotation, targetRot) * Mathf.Deg2Rad;
        // Distance to decelerate from our current speed (In radians).
        float distanceToDecelerateFromCurrentSpeed = (_currentRotationSpeed * _currentRotationSpeed) / (2f * _rotationDeceleration);

        #region Debug Logs
        if (_displayDebug)
        {
            if (_debugAngles)
            {
                Debug.Log("Distance Remaining (Deg): " + distanceRemaining * Mathf.Rad2Deg);
                Debug.Log("Distance To Decelerate (Deg): " + distanceToDecelerateFromCurrentSpeed * Mathf.Rad2Deg);
            }
            else
            {
                Debug.Log("Distance Remaining (Rad): " + distanceRemaining);
                Debug.Log("Distance To Decelerate (Rad): " + distanceToDecelerateFromCurrentSpeed);
            }
        }
        #endregion

        // Accelerate while the distance remaining is greater than the distance to decelerate from our current speed.
        if (distanceRemaining > distanceToDecelerateFromCurrentSpeed)
        {
            // Accelerate to Max Speed (Or maintain speed if at max).
            _currentRotationSpeed = Mathf.Min(_currentRotationSpeed + (_rotationAcceleration * Time.deltaTime), _maxRotationSpeed);
        }
        else
        {
            // Decelerate towards 0.
            _currentRotationSpeed = Mathf.Max(_currentRotationSpeed - (_rotationDeceleration * Time.deltaTime), 0);
        }

        // Rotate towards the target rotation at our current speed.
        Vector3 newDirection = Vector3.RotateTowards(_rotationTarget.forward, targetDir, _currentRotationSpeed * Time.deltaTime, 0.0f);
        _rotationTarget.rotation = Quaternion.LookRotation(newDirection);


        // If we are looking close enough to the current target's direction, move onto the next target.
        if (Vector3.Angle(targetDir, _rotationTarget.forward) < _foundTargetBuffer)
        {
            _rotationPauseRemaining = _rotationPause;
            _currentRotationSpeed = 0f;

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
