using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    [HideInInspector] public Transform Target;
    private bool _withinShootingAngle;


    [SerializeField] private HealthComponent _healthComponent;
    [SerializeField] private EnemySenses _senses;

    [Space(5)]
    public float DetectionCheckDelay = 0.2f;

    [Header("Rotation Variables")]
    [SerializeField] private Transform _rotationTarget;
    [SerializeField] private float _rotationSpeed;

    [Header("Idle State Variables")]
    [SerializeField] private Vector3[] _idleLookTargets;
    [SerializeField] private float _idleRotationSpeed;
    [SerializeField] private float _idleRotationPause;

    [Header("Alert State Variables")]
    [SerializeField] private ParticleSystem _alertPS;

    [Header("Shooting State Variables")]
    [SerializeField] private LayerMask _shotObstructionLayers;
    [SerializeField] private float _damage;
    [SerializeField] private int _burstCount;
    [SerializeField] private float _burstDelay;

    [SerializeField] private float _fireDelay;
    [SerializeField] private float _fireRotationDelay;

    [Header("Deactivated State Variables")]
    [SerializeField] private float _deactivationDuration = 2f;


    private StateMachine _stateMachine;
    private void Awake()
    {
        _stateMachine = new StateMachine();

        var idle = new TurretIdle(this, _rotationTarget, _idleLookTargets, _idleRotationSpeed, _idleRotationPause);
        var alert = new TurretAlert(this, _rotationTarget, _rotationSpeed, _alertPS);
        var shooting = new TurretShooting(this, _rotationTarget, _rotationSpeed, _fireDelay, _fireRotationDelay, _shotObstructionLayers, _damage, _burstCount, _burstDelay);
        var deactivated = new TurretDeactivated(_healthComponent, _deactivationDuration);

        #region Transitions
        // Any.
        _stateMachine.AddAnyTransition(deactivated, OutOfHealth());
        //At(idle, deactivated, OutOfHealth());

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
        Func<bool> LostTarget() => () => (Target == null) && (alert.FacingTargetDirection);
        Func<bool> TargetWithinFireCone() => () => Target != null && _withinShootingAngle;
        Func<bool> TargetOutwithinFireCone() => () => Target != null && !_withinShootingAngle;
        Func<bool> OutOfHealth() => () => _healthComponent.CurrentHealth <= 0;
        Func<bool> ReactivationTimeElapsed() => () => deactivated.DeactivationTimeElapsed == true;
        #endregion
    }


    private void Update()
    {
        _stateMachine.Tick();
    }


    public void TryGetTarget()
    {
        Target = _senses.TryGetTarget(out _withinShootingAngle);
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < _idleLookTargets.Length; i++)
        {
            Gizmos.DrawSphere(_idleLookTargets[i], 0.25f);
        }
    }
}
