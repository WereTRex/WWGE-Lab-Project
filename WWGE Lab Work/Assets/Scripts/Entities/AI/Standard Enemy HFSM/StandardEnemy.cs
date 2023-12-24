using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;
using EnemyStates.Standard;
using System;
using UnityEngine.AI;

public class StandardEnemy : SpawnableEntity, IStaggerable
{
    private StateMachine<Action> _rootFSM;
    [ReadOnly] public string CurrentStatePath;

    private bool _isInside = false;
    private Action OnStaggered;
    private Coroutine _searchForTargetCoroutine;


    [Header("General")]
    [SerializeField] private HealthComponent _healthComponent;

    [SerializeField] private float _tauntDuration;
    [SerializeField] private float _staggerDuration;


    [Header("Movement")]
    [SerializeField] private NavMeshAgent _agent;


    [Header("Targeting")]
    //[SerializeField] private Transform _player;
    [ReadOnly, SerializeField] private Transform _currentTarget;
    [ReadOnly, SerializeField] private Vector3? _suspiciousPosition = null;
    //private Transform _currentTargetProperty => _currentTarget == null ? _player : _currentTarget;
    
    [SerializeField] private EnemySenses _senses;
    private RepairableBarrier _initialTarget;

    private const float TARGET_DETECTION_DELAY = 0.2f;


    [Header("Wandering")]
    [SerializeField] private float _maxWanderDistance;
    [SerializeField] private float _wanderStoppingDistance;
    [SerializeField] private float _wanderMinIdleTime;
    [SerializeField] private float _wanderMaxIdleTime;


    [Header("Attacking")]
    [SerializeField] private HurtboxAttack[] _attacks;
    [SerializeField] private float _attackRotationSpeed;
    [SerializeField] private float _attackStoppingDistance;


    [Header("Debug")]
    [SerializeField] private bool _drawGizmos;
    [SerializeField] private int _attackToDisplay;
    private Timer timeAlive;



    private void Awake()
    {
        #region State Machine Setup
        _rootFSM = new StateMachine<Action>();


        // Root States.
        var outsideFSM = new Outside();
        var tauntState = new Taunting(_tauntDuration);
        var insideFSM = new Inside(this);
        var staggerState = new Stagger(_staggerDuration);
        var deadState = new Dead(this.gameObject, 1f);

        // Outside Sub-States.
        var waitingForTarget = new WaitingForInitialTarget(hasTarget: () => _initialTarget != null);
        var moveToBarrier = new MovingToBarrier(_agent, () => _initialTarget.transform);
        var attackingBarrier = new AttackingBarrier(this, _attacks, () => _initialTarget.transform, _agent, _attackStoppingDistance);

        // Inside Sub-States.
        var wanderState = new Wander(_agent, _wanderStoppingDistance, _maxWanderDistance, _wanderMinIdleTime, _wanderMaxIdleTime);
        var investigatePosition = new InvestigatePosition(_agent, () => _suspiciousPosition.Value);
        var moveToTarget = new MovingToTarget(_agent, () => _currentTarget);
        var attackingTarget = new AttackingTarget(this, _attacks, () => _currentTarget, _agent, _attackRotationSpeed, _attackStoppingDistance);



        // Setup Root FSM.
        _rootFSM.AddState(outsideFSM);
        _rootFSM.AddState(tauntState);
        _rootFSM.AddState(insideFSM);
        _rootFSM.AddState(staggerState);
        _rootFSM.AddState(deadState);

        #region RootFSM Transitions
        // Transition to the Taunt State when we reach our initial target AND the initial target is not blocking us (Has no health).
        _rootFSM.AddTransition(
            from: outsideFSM,
            to: tauntState,
            condition: t => _initialTarget != null && (Vector3.Distance(transform.position, _initialTarget.transform.position) <= _attackStoppingDistance + 0.5f) && (!_initialTarget.IsActive),
            onTransition: t => _isInside = true);
        // Also transition to the Taunt State if we have no initial target after being alive for 5 seconds.
        _rootFSM.AddTransition(
            from: outsideFSM,
            to: tauntState,
            condition: t => _initialTarget == null && timeAlive.Elapsed > 5f,
            onTransition: t => _isInside = true);

        // Transition to Inside once the Taunt State has completed (The Taunt State prevents transitions till it has elapsed).
        _rootFSM.AddTransition(
            from: tauntState,
            onTransition: t => EnableNavMeshAgent(),
            to: insideFSM);

        // Transitions TO Stagger.
        _rootFSM.AddTriggerTransition(
            from: outsideFSM,
            to: staggerState,
            trigger: ref OnStaggered,
            onTransition: t => DisableNavMeshAgent(),
            forceInstantly: true);
        _rootFSM.AddTriggerTransition(
            from: tauntState,
            to: staggerState,
            trigger: ref OnStaggered,
            onTransition: t => DisableNavMeshAgent(),
            forceInstantly: true);
        _rootFSM.AddTriggerTransition(
            from: insideFSM,
            to: staggerState,
            trigger: ref OnStaggered,
            onTransition: t => DisableNavMeshAgent(),
            forceInstantly: true);

        // Transitions FROM Stagger (The stagger state itself stops transitions till the stagger has elapsed).
        _rootFSM.AddTransition(
            from: staggerState,
            to: outsideFSM,
            onTransition: t => EnableNavMeshAgent(),
            condition: t => !_isInside);
        _rootFSM.AddTransition(
            from: staggerState,
            to: insideFSM,
            onTransition: t => EnableNavMeshAgent(),
            condition: t => _isInside);


        // Instantly transition to the Dead state from any state if we are out of health.
        _rootFSM.AddAnyTransition(
            to: deadState,
            condition: t => _healthComponent.HasHealth == false,
            onTransition: t => DisableNavMeshAgent(),
            forceInstantly: true);
        #endregion


        // Setup Outside Sub-FSM.
        outsideFSM.AddState(waitingForTarget);
        outsideFSM.AddState(moveToBarrier);
        outsideFSM.AddState(attackingBarrier);

        #region OutsideFSM Transitions
        outsideFSM.AddTransition(
            from: waitingForTarget,
            to: moveToBarrier);
        outsideFSM.AddTwoWayTransition(
            from: moveToBarrier,
            to: attackingBarrier,
            condition: t => Vector3.Distance(transform.position, _initialTarget.transform.position) <= attackingBarrier.MaxAttackRange);
        #endregion


        // Setup Inside Sub-FSM.
        insideFSM.AddState(wanderState);
        insideFSM.AddState(investigatePosition);
        insideFSM.AddState(moveToTarget);
        insideFSM.AddState(attackingTarget);

        #region InsideFSM Transitions
        // Transition to the moveToTarget state if we find a target while wandering.
        insideFSM.AddTransition(
            from: wanderState,
            to: moveToTarget,
            condition: t => _currentTarget != null);

        // Transition to the investigatePosition state from wandering if we have a suspicious point.
        insideFSM.AddTransition(
            from: wanderState,
            to: investigatePosition,
            condition: t => _suspiciousPosition != null);


        // Transition to the Wandering state from Investigating if we no longer have a suspicious point.
        insideFSM.AddTransition(
            from: investigatePosition,
            to: wanderState,
            condition: t => _suspiciousPosition == null);
        insideFSM.AddTransition(
            from: investigatePosition,
            to: moveToTarget,
            condition: t => _currentTarget != null);


        // Transition between the moveToTarget and attackingTarget states depending on whether the target is within the range of our attacks.
        insideFSM.AddTwoWayTransition(
            from: moveToTarget,
            to: attackingTarget,
            condition: t => Vector3.Distance(transform.position, _currentTarget.position) <= attackingTarget.MaxAttackRange);


        // Transition to the wander state if we have no target.
        insideFSM.AddAnyTransition(
            to: wanderState,
            condition: t => _currentTarget == null);
        #endregion


        _rootFSM.SetStartState(outsideFSM);
        outsideFSM.SetStartState(waitingForTarget);
        insideFSM.SetStartState(moveToTarget);

        _rootFSM.Init();
        #endregion

        timeAlive = new Timer();
    }

    private void OnEnable() => _healthComponent.OnHealthDecreased += OnDamaged;
    private void OnDisable() => _healthComponent.OnHealthDecreased -= OnDamaged;
    



    private void EnableNavMeshAgent() => _agent.enabled = true;
    private void DisableNavMeshAgent() => _agent.enabled = false;


    private void Update()
    {
        // Tick the Root FSM.
        _rootFSM.OnTick();

        if (_suspiciousPosition.HasValue && Vector3.Distance(transform.position, _suspiciousPosition.Value) <= _agent.stoppingDistance + 0.5f)
            _suspiciousPosition = null;

        // Update Debug Info.
        CurrentStatePath = _rootFSM.GetActiveHierarchyPath();
    }


    public void StartDetection()
    {
        StopDetection();
        _searchForTargetCoroutine = StartCoroutine(SearchForTarget());
    }
    public void StopDetection()
    {
        if (_searchForTargetCoroutine != null)
            StopCoroutine(_searchForTargetCoroutine);
    }
    private IEnumerator SearchForTarget()
    {
        while (true)
        {
            // Detect a new target only if they are within the secondary sight area.
            Transform potentialTarget = _senses.TryGetTarget(out bool withinSight);
            if (_currentTarget != null || withinSight)
                _currentTarget = potentialTarget;


            yield return new WaitForSeconds(TARGET_DETECTION_DELAY);
        }
    }


    public override void SetInitialTarget(Transform target) => _initialTarget = target.GetComponent<RepairableBarrier>();
    public void EnteredBuilding() => _isInside = true;


    public void Stagger() => OnStaggered?.Invoke();


    private void OnDamaged(Vector3 origin, float newValue) => _suspiciousPosition = origin;



    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos)
            return;


        Gizmos.color = Color.red;
        _attackToDisplay = Mathf.Clamp(_attackToDisplay, 0, _attacks.Length - 1);

        // (Gizmo) Display the maxAttackRange.
        float maxAttackRange = _attacks[_attackToDisplay].GetAttackRange();
        Gizmos.DrawWireSphere(transform.position, maxAttackRange);

        // (Gizmo) Display the attackAngle.
        float maxAttackAngle = _attacks[_attackToDisplay].GetAttackAngle();
        Vector3 leftDirection = Quaternion.AngleAxis(-(maxAttackAngle / 2f), transform.up) * transform.forward;
        Vector3 rightDirection = Quaternion.AngleAxis(maxAttackAngle / 2f, transform.up) * transform.forward;

        Gizmos.DrawRay(transform.position, leftDirection * maxAttackRange);
        Gizmos.DrawRay(transform.position, rightDirection * maxAttackRange);
    }
}
