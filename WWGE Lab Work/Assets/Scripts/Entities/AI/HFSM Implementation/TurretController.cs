using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;
using WwGEProject.AI.Turret;

public class TurretController : MonoBehaviour
{
    // Declare the FSM.
    private StateMachine<Action> _rootFSM;

    // Parameters.
    [ReadOnly] public string CurrentStatePath;
    [ReadOnly] public string CurrentState;
    [SerializeField] private Transform Target;
    [SerializeField] private float _currentHealth;
    [SerializeField] private bool _withinShootingRadius;


    private void Start()
    {
        #region State Machine Setup
        _rootFSM = new StateMachine<Action>();
        var awareFSM = new TurretAwareState<Action>();
        var idleState = new TurretIdleState();
        var deactivatedState = new TurretDeactivatedState();

        var alertState = new TurretAlertState();
        var shootingState = new TurretShootingState();


        // Setup root FSM.
        _rootFSM.AddState(idleState);
        _rootFSM.AddState(awareFSM);
        _rootFSM.AddState(deactivatedState);

        _rootFSM.AddTransition(from: idleState, to: awareFSM, condition: transition => Target != null, onTransition: transition => PlayAlertEffects());
        _rootFSM.AddTransition(from: awareFSM, to: idleState, condition: t => Target == null);
        _rootFSM.AddTransitionFromAny(to: deactivatedState, condition: transition => _currentHealth <= 0);

        
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
        _rootFSM.OnTick();
        CurrentState = _rootFSM.ActiveStateName;
        CurrentStatePath = _rootFSM.GetActiveHierarchyPath();
    }

    private void PlayAlertEffects()
    {
        Debug.Log("Entered Alert State");
    }
}
