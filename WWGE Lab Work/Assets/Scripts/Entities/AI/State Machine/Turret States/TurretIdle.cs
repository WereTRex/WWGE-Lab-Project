using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A idle state for the turret.</summary>
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
    private float _foundTargetBuffer = 2f;


    public TurretIdle(EnemyTurret brain, Transform rotationTarget, Vector3[] lookPoints, float rotationSpeed, float rotationAcceleration, float rotationDeceleration, float rotationPauseTime)
    {
        // Set readonly variables.
        this._brain = brain;
        this._rotationTarget = rotationTarget;

        this._lookPoints = lookPoints;
        
        this._maxRotationSpeed = rotationSpeed;
        this._rotationAcceleration = rotationAcceleration;
        this._rotationDeceleration = rotationDeceleration;
        
        this._rotationPause = rotationPauseTime;
    }


    public void Tick()
    {
        // Decrement Pause Delay.
        if (_rotationPauseRemaining > 0f) {
            _rotationPauseRemaining -= Time.deltaTime;
            return;
        }

        // Calculate the direction & rotation to the current target point.
        Vector3 lookPointPos = _brain.transform.position + _lookPoints[_rotationIndex];
        Vector3 targetDir = (lookPointPos - _rotationTarget.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        
        // Rotate towards the next point.
        AccelerateTowardsPoint(targetDir, targetRot);


        // If we are looking close enough to the current target's direction, move onto the next target.
        if (Vector3.Angle(targetDir, _rotationTarget.forward) < _foundTargetBuffer)
        {
            // Set the duration remaining.
            _rotationPauseRemaining = _rotationPause;
            _currentRotationSpeed = 0f;

            // Increment the rotation index (Looping to 0 to avoid exceeding it).
            if (_rotationIndex < _lookPoints.Length - 1)
                _rotationIndex++;
            else
                _rotationIndex = 0;
        }
    }

    private void AccelerateTowardsPoint(Vector3 targetDir, Quaternion targetRot)
    {
        // Calculate the angle to the target (In radians).
        float distanceRemaining = Quaternion.Angle(_rotationTarget.rotation, targetRot) * Mathf.Deg2Rad;

        // Calculate the angle required to decelerate to a stop from our current speed (In radians).
        float distanceToDecelerateFromCurrentSpeed = (_currentRotationSpeed * _currentRotationSpeed) / (2f * _rotationDeceleration);

        #region Debug Logs
        if (_brain.UseDebugLogs)
        {
            Debug.Log(string.Format("Distance Remaining: {0} (Rad), {1} (Deg)", distanceRemaining, distanceRemaining * Mathf.Rad2Deg));
            Debug.Log(string.Format("Distance To Decelerate: {0} (Rad), {1} (Deg)", distanceToDecelerateFromCurrentSpeed, distanceToDecelerateFromCurrentSpeed * Mathf.Rad2Deg));
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
            // Decelerate towards a stop.
            _currentRotationSpeed = Mathf.Max(_currentRotationSpeed - (_rotationDeceleration * Time.deltaTime), 0);
        }

        // Rotate towards the target rotation at our current speed.
        Vector3 newDirection = Vector3.RotateTowards(_rotationTarget.forward, targetDir, _currentRotationSpeed * Time.deltaTime, 0.0f);
        _rotationTarget.rotation = Quaternion.LookRotation(newDirection);
    }


    public void OnEnter()
    {
        _brain.StartDetection();

        _rotationIndex = 0;
        _rotationPauseRemaining = 0;
    }
    public void OnExit()
    {
        _brain.StopDetection();
    }
}
