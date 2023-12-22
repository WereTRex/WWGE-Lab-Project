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

    [Header("General")]

    [SerializeField] private Transform _target;
    private RepairableBarrier _initialTarget;

    [SerializeField] private HealthComponent _healthComponent;

    [SerializeField] private float _tauntDuration;
    [SerializeField] private float _staggerDuration;


    [Header("Movement")]
    [SerializeField] private NavMeshAgent _agent;


    [Header("Attacking")]
    [SerializeField] private HurtboxAttack[] _attacks;
    [SerializeField] private float _attackRotationSpeed;
    [SerializeField] private float _attackStoppingDistance;


    [Header("Debug")]
    [SerializeField] private bool _drawGizmos;
    private Timer timeAlive;



    private void Awake()
    {
        #region State Machine Setup
        _rootFSM = new StateMachine<Action>();


        // Root States.
        var outsideFSM = new Outside();
        var tauntState = new Taunting(_tauntDuration);
        var insideFSM = new Inside();
        var staggerState = new Stagger(_staggerDuration);
        var deadState = new Dead(this.gameObject, 1f);

        // Outside Sub-States.
        var waitingForTarget = new WaitingForInitialTarget(hasTarget: () => _initialTarget != null);
        var moveToBarrier = new MovingToBarrier(_agent, () => _initialTarget.transform);
        var attackingBarrier = new AttackingBarrier(this, _attacks, () => _initialTarget.transform, _agent, _attackStoppingDistance);
        
        // Inside Sub-States.
        var moveToTarget = new MovingToTarget(_agent, () => _target);
        var attackingTarget = new AttackingTarget(this, _attacks, () => _target, _agent, _attackRotationSpeed, _attackStoppingDistance);



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
        _rootFSM.AddTransitionFromAny(
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
        insideFSM.AddState(moveToTarget);
        insideFSM.AddState(attackingTarget);

        #region InsideFSM Transitions
        insideFSM.AddTwoWayTransition(
            from: moveToTarget,
            to: attackingTarget,
            condition: t => Vector3.Distance(transform.position, _target.position) <= attackingTarget.MaxAttackRange);
        #endregion


        _rootFSM.SetStartState(outsideFSM);
        outsideFSM.SetStartState(waitingForTarget);
        insideFSM.SetStartState(moveToTarget);
        _rootFSM.Init();
        #endregion


        timeAlive = new Timer();
    }


    private void EnableNavMeshAgent() => _agent.enabled = true;
    private void DisableNavMeshAgent() => _agent.enabled = false;


    private void Update()
    {
        // Tick the Root FSM.
        _rootFSM.OnTick();

        // Update Debug Info.
        CurrentStatePath = _rootFSM.GetActiveHierarchyPath();
    }


    public override void SetInitialTarget(Transform target) => _initialTarget = target.GetComponent<RepairableBarrier>();
    public void EnteredBuilding() => _isInside = true;


    public void Stagger() => OnStaggered?.Invoke();



    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < _attacks.Length; i++)
        {
            Gizmos.DrawWireSphere(transform.position, _attacks[i].GetAttackRange());
        }
    }
}
