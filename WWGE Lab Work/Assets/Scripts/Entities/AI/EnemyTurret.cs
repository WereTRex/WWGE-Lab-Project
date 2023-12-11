using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    // Debug Variables.
    [ReadOnly] public Transform Target;
    [ReadOnly] public int ID;
    [ReadOnly] public string CurrentStateName;


    [Space(15)]


    [SerializeField] private HealthComponent _healthComponent;
    [SerializeField] private EnemySenses _senses;
    private bool _withinShootingAngle;

    [Space(5)]
    public float DetectionCheckDelay = 0.2f;
    private Coroutine _detectionCoroutine;

    [Header("Rotation Variables")]
    [SerializeField] private Transform _rotationTarget;
    [SerializeField] private float _rotationSpeed;

    [Header("Idle State Variables")]
    [SerializeField] private Vector3[] _idleLookTargets;
    [SerializeField] private float _idleRotationSpeed;
    [SerializeField] private float _idleRotationAcceleration;
    [SerializeField] private float _idleRotationDeceleration;
    [SerializeField] private float _idleRotationPause;

    [Header("Alert State Variables")]
    [SerializeField] private float _minimumAlertDuration = 1f;
    [SerializeField] private ParticleSystem _alertPS;

    [Header("Shooting State Variables")]
    [SerializeField] private Gun _turretGun;
    [SerializeField] private bool _pauseWhileShooting;

    [Header("Deactivated State Variables")]
    [SerializeField] private Repairable _repairableComponent;


    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = false;
    public bool DrawDebug => _drawGizmos;
    [field:SerializeField] public bool UseDebugLogs { get; private set; }


    private StateMachine _stateMachine;
    private void Awake()
    {
        if (_senses == null)
            _senses = GetComponent<EnemySenses>();

        
        #region State Machine Setup
        _stateMachine = new StateMachine();
        ID = _stateMachine.ID;

        var idle = new TurretIdle(this, _rotationTarget, _idleLookTargets, _idleRotationSpeed, _idleRotationAcceleration, _idleRotationDeceleration, _idleRotationPause);
        var alert = new TurretAlert(this, _rotationTarget, _rotationSpeed, _minimumAlertDuration, _alertPS);
        var shooting = new TurretShooting(this, _rotationTarget, _rotationSpeed, _turretGun, _pauseWhileShooting);
        var deactivated = new TurretDeactivated(this, _repairableComponent);

        #region Transitions
        // Any.
        _stateMachine.AddAnyTransition(deactivated, OutOfHealth());

        // Idle.
        At(idle, alert, HasTarget());

        // Alert.
        At(alert, idle, LostTarget());
        At(alert, shooting, TargetWithinFireCone());

        // Shooting.
        At(shooting, alert, TargetOutwithinFireCone());

        // Deactivated.
        At(deactivated, idle, ReactivationTimeElapsed());
        #endregion


        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
        
        _stateMachine.SetState(idle);
        
        #region Transition Conditions
        Func<bool> HasTarget() => () => Target != null;
        Func<bool> LostTarget() => () => Target == null && alert.FacingTargetDirection && alert.MinimumDurationElapsed;
        Func<bool> TargetWithinFireCone() => () => Target != null && _withinShootingAngle;
        Func<bool> TargetOutwithinFireCone() => () => Target == null || !_withinShootingAngle;
        Func<bool> OutOfHealth() => () => _healthComponent.CurrentHealthProperty <= 0;
        Func<bool> ReactivationTimeElapsed() => () => _healthComponent.HasHealth;
        #endregion
        #endregion
    }


    private void Update()
    {
        // Call the State Machine's tick method.
        _stateMachine.Tick();

        // Update the CurrentStateName debug variable.
        CurrentStateName = _stateMachine.GetCurrentStateName();
    }
    


    public void TryGetTarget()
    {
        Target = _senses.TryGetTarget(out _withinShootingAngle);
    }
    public void StartDetection()
    {
        // Ensure there is no currently running detection coroutine.
        StopDetection();
        
        // Start the detection coroutine.
        _detectionCoroutine = StartCoroutine(TryDetection());
    }
    public void StopDetection()
    {
        // Stop the currently running detection coroutine (If there is one).
        if (_detectionCoroutine != null)
            StopCoroutine(_detectionCoroutine);
    }
    private IEnumerator TryDetection()
    {
        // Loop until externally stopped.
        while (true)
        {
            // Note: Done in 2 lines as instantly setting the target seemed to cause an error occasionally.
            Transform newTarget = _senses.TryGetTarget(out _withinShootingAngle);
            Target = newTarget;

            // Rather than attempting every frame, wait a number of seconds equal to the DetectionCheckDelay float.
            yield return new WaitForSeconds(DetectionCheckDelay);
        }
    }



    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos)
            return;
        
        // A Gizmo showing the location of the idle look targets relative to this turret.
        Gizmos.color = Color.green;
        for (int i = 0; i < _idleLookTargets.Length; i++)
        {
            Gizmos.DrawSphere(transform.position + _idleLookTargets[i], 0.25f);
        }
    }
}
