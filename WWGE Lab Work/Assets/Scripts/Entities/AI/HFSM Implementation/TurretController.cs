using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;

public class TurretController : MonoBehaviour
{
    // Declare the FSM.
    private StateMachine<string, TurretStates, Events> _rootFSM;

    // Parameters.
    [ReadOnly] public string CurrentStatePath;
    [ReadOnly] public string CurrentState;
    [SerializeField] private Transform Target;
    [SerializeField] private float _currentHealth;
    [SerializeField] private bool _withinShootingRadius;


    enum TurretStates { 
        Idle, Aware, Deactivated
    }
    enum AwareStates {
        Alert, Shooting
    }
    enum Events {
        OnDamage, OnDead
    }

    private void Start()
    {
        #region State Machine Setup
        _rootFSM = new StateMachine<string, TurretStates, Events>();
        var awareFSM = new StateMachine<TurretStates, AwareStates, Events>();
        var idleState = new State<TurretStates, Events>();
        var deactivatedState = new State<TurretStates, Events>();

        // Setup root FSM.
        _rootFSM.AddState(TurretStates.Idle, idleState);
        _rootFSM.AddState(TurretStates.Aware, awareFSM);
        _rootFSM.AddState(TurretStates.Deactivated, deactivatedState);

        _rootFSM.AddTransition(from: TurretStates.Idle, to: TurretStates.Aware, condition: transition => Target != null, onTransition: transition => PlayAlertEffects());
        _rootFSM.AddTransition(from: TurretStates.Aware, to: TurretStates.Idle, condition: t => Target == null);
        _rootFSM.AddTransitionFromAny(to: TurretStates.Deactivated, condition: transition => _currentHealth <= 0);


        // Setup Aware FSM.
        awareFSM.AddState(AwareStates.Alert);
        awareFSM.AddState(AwareStates.Shooting);
        //awareFSM.AddTransition(from: AwareStates.Alert, to: AwareStates.Shooting, condition: PlayerWithinShootingAngle());
        awareFSM.AddTwoWayTransition(from: AwareStates.Alert, to: AwareStates.Shooting, condition: transition => _withinShootingRadius); // Transition from Alert to Shooting when player is within shooting radius.


        // Initialise the FSM.
        _rootFSM.SetStartState(TurretStates.Idle);
        awareFSM.SetStartState(AwareStates.Alert);

        _rootFSM.Init();
        #endregion
    }


    private void Update()
    {
        _rootFSM.OnLogic();
        CurrentState = _rootFSM.ActiveStateName.ToString();
        CurrentStatePath = _rootFSM.GetActiveHierarchyPath();
    }

    private void PlayAlertEffects()
    {
        Debug.Log("Entered Alert State");
    }
}
