using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;
using WwGEProject.AI.Turret;

public class TurretController : MonoBehaviour
{
    private StateMachine<Action> _rootFSM;

    
    [field: SerializeField, ReadOnly] public Transform Target { get; private set; }

    [ReadOnly] public string CurrentStatePath;
    [ReadOnly] public string CurrentState;

    private Coroutine _detectionCoroutine;
    private const float DETECTION_CHECK_DELAY = 0.15f;


    [Header("Assorted")]
    [SerializeField] private HealthComponent _healthComponent;
    [SerializeField] private EnemySenses _senses;
    private bool _withinShootingRadius;

    [SerializeField] private float _targetLossAngleMultiplier = 1.2f;

    
    [Header("Movement & Rotation")]
    [SerializeField] private Transform _rotationTarget;


    
    private Quaternion _currentTargetRotation;
    private float _currentTargetRotationSpeed;
    private float _currentRotationSpeed;


    [Header("Idle State Parameters")]
    [SerializeField] private Quaternion[] _idleTargetRotations;
    [SerializeField] private float _idleRotationPause;

    [Space(5)]

    [SerializeField] private float _idleRotationSpeed = 45f;
    [SerializeField] private float _idleRotationAcceleration = 75f;
    [SerializeField] private float _idleRotationDeceleration = 45f;


    [Header("Aware State Parameters")]
    [SerializeField] private float _detectionLostTime = 0.6f;


    [Header("Alert State Parameters")]
    [SerializeField] private float _alertInitialPause;
    [SerializeField] private ParticleSystem _awareEnterPS;

    [Space(5)]

    [SerializeField] private float _alertRotationSpeed;
    [SerializeField] private float _alertRotationAcceleration;
    [SerializeField] private float _alertRotationDeceleration;


    [Header("Shooting State Parameters")]
    [SerializeField] private Gun _turretGun;

    [SerializeField] private float _shootingMaxSpeed;
    [SerializeField] private float _shootingRotationAcceleration;
    [SerializeField] private float _shootingRotationDeceleration;


    [Header("Deactivated State Parameters")]
    [SerializeField] private Repairable _repairableComponent;


    [Header("Draw Gizmos")]
    [SerializeField] private bool _drawGizmos;



    private void Start()
    {
        #region State Machine Setup
        _rootFSM = new StateMachine<Action>();
        var idleState = new TurretIdleState(this, _rotationTarget, _idleTargetRotations, _idleRotationSpeed, _idleRotationAcceleration, _idleRotationDeceleration, _idleRotationPause);
        var awareFSM = new TurretAwareState(this, _detectionLostTime);
        var deactivatedState = new TurretDeactivatedState(_repairableComponent);

        var alertState = new TurretAlertState(this, _alertRotationSpeed, _alertRotationAcceleration, _alertRotationDeceleration, _alertInitialPause);
        var shootingState = new TurretShootingState(this, _turretGun, _shootingMaxSpeed, _shootingRotationAcceleration, _shootingRotationDeceleration);


        // Setup root FSM.
        _rootFSM.AddState(idleState);
        _rootFSM.AddState(awareFSM);
        _rootFSM.AddState(deactivatedState);

        #region Root FSM Transitions
        _rootFSM.AddTransition(
            from: idleState,
            to: awareFSM,
            condition: t => Target != null,
            onTransition: transition => PlayAlertEffects());
        _rootFSM.AddTransition(
            from: awareFSM,
            to: idleState,
            condition: t => Target == null);
        _rootFSM.AddTransitionFromAny(
            to: deactivatedState,
            condition: t => _healthComponent.HasHealth == false,
            onTransition: t => ResetRotationSpeeds(),
            forceInstantly: true);
        _rootFSM.AddTransition(
            from: deactivatedState,
            to: idleState,
            condition: t => _healthComponent.HasFullHealth);
        #endregion



        // Setup Aware Sub-FSM.
        awareFSM.AddState(alertState);
        awareFSM.AddState(shootingState);
        awareFSM.AddTwoWayTransition(from: alertState, to: shootingState, condition: transition => _withinShootingRadius); // Transition from Alert to Shooting when player is within shooting radius.


        // Initialise the FSM.
        _rootFSM.SetStartState(idleState);
        awareFSM.SetStartState(alertState);

        _rootFSM.Init();
        #endregion
    }


    private void Update()
    {
        // Tick the Root FSM.
        _rootFSM.OnTick();


        RotateToCurrentTarget();


        // Set Debug Info.
        CurrentState = _rootFSM.ActiveStateName;
        CurrentStatePath = _rootFSM.GetActiveHierarchyPath();
    }


    #region Rotate To Target Functions
    /// <summary> Rotate towards a passed target, accelerating and decelerating as necessary.</summary>
    public void RotateToTarget(Quaternion targetRotation, float maxSpeed, float acceleration, float deceleration)
    {
        // Note: For the first time that this turret pans to the target rotation, it will snap to it at the last moment. Only happens the first time though.
        #region TargetRotation
        float angleToTargetRemaining = Quaternion.Angle(_currentTargetRotation, targetRotation);
        float angleForTargetToDecelerate = (_currentTargetRotationSpeed * _currentTargetRotationSpeed) / (2f * deceleration);


        // If our current speed is greater than the max speed (Something that will only happen when changing states), then decelerate to the max speed.
        if (_currentTargetRotationSpeed > maxSpeed)
        {
            _currentTargetRotationSpeed = Mathf.Max(_currentTargetRotationSpeed - (deceleration * Time.deltaTime), maxSpeed);
        }
        // Accelerate while the distance remaining is greater than the distance to decelerate from our current speed.
        else if (angleToTargetRemaining > angleForTargetToDecelerate)
        {
            // Accelerate to Max Speed (Or maintain speed if at max).
            _currentTargetRotationSpeed = Mathf.Min(_currentTargetRotationSpeed + (acceleration * Time.deltaTime), maxSpeed);
        }
        else
        {
            // Decelerate towards a stop.
            _currentTargetRotationSpeed = Mathf.Max(_currentTargetRotationSpeed - (deceleration * Time.deltaTime), 0);
        }

        _currentTargetRotation = Quaternion.RotateTowards(_currentTargetRotation, targetRotation, _currentTargetRotationSpeed * Time.deltaTime);
        #endregion


        #region Rotation Target Rotation
        // Calculate the angle to the current rotation target (In Radians).
        float angleRemaining = Quaternion.Angle(_rotationTarget.rotation, _currentTargetRotation);

        // Calculate the angle requred to decelerate to 0 from our current speed (In radians).
        float angleToDecelerate = (_currentRotationSpeed * _currentRotationSpeed) / (2f * deceleration);


        // If our current speed is greater than the max speed (Something that will only happen when changing states), then decelerate to the max speed.
        if (_currentRotationSpeed > maxSpeed)
        {
            _currentRotationSpeed = Mathf.Max(_currentRotationSpeed - (deceleration * Time.deltaTime), maxSpeed);
        }
        // Accelerate while the distance remaining is greater than the distance to decelerate from our current speed.
        else if (angleRemaining > angleToDecelerate)
        {
            // Accelerate to Max Speed (Or maintain speed if at max).
            _currentRotationSpeed = Mathf.Min(_currentRotationSpeed + (acceleration * Time.deltaTime), maxSpeed);
        }
        else
        {
            // Decelerate towards a stop.
            _currentRotationSpeed = Mathf.Max(_currentRotationSpeed - (deceleration * Time.deltaTime), 0);
        }
        #endregion
    }
    /// <summary> Rotate towards a passed target, accelerating and decelerating as necessary.</summary>
    public void RotateToTarget(Vector3 targetPos, float maxSpeed, float acceleration, float deceleration)
    {
        // Accelerate CurrentRotationTarget towards the rotation to the current target.
        Vector3 targetDirection = (targetPos - _rotationTarget.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        RotateToTarget(targetRotation, maxSpeed, acceleration, deceleration);
    }
    public void ResetRotationSpeeds()
    {
        _currentTargetRotationSpeed = 0f;
        _currentRotationSpeed = 0f;
    }

    
    // Rotate towards the target rotation at our current speed.
    private void RotateToCurrentTarget()
    {
        Quaternion newRotation = Quaternion.RotateTowards(_rotationTarget.rotation, _currentTargetRotation, _currentRotationSpeed * Time.deltaTime);
        _rotationTarget.rotation = newRotation;
    }
    #endregion


    public void TryGetTarget()
    {
        if (Target == null)
            Target = _senses.TryGetTarget(out _withinShootingRadius);
        else
            Target = _senses.TryGetTarget(out _withinShootingRadius, angleMultiplier: _targetLossAngleMultiplier);
    }

    private void PlayAlertEffects()
    {
        Debug.Log("Entered Alert State");
        _awareEnterPS.Play();
    }



    private void OnDrawGizmos()
    {
        if (!_drawGizmos)
            return;


        // Idle Rotation Directions.
        Gizmos.color = Color.blue;
        foreach (Quaternion targetRot in _idleTargetRotations)
        {
            Gizmos.DrawRay(_rotationTarget.position, targetRot * Vector3.forward * 1.5f);
        }


        Gizmos.color = Color.red;
        Gizmos.DrawRay(_rotationTarget.position, (_currentTargetRotation * Vector3.forward).normalized * 1.5f);

        if (Target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(_rotationTarget.position, Quaternion.LookRotation((Target.position - _rotationTarget.position).normalized) * Vector3.forward * 1.5f);
        }
    }
}
