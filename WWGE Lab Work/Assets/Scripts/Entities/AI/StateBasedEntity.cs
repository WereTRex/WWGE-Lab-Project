using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateBasedEntity : MonoBehaviour
{
    // Debug Variables.
    [ReadOnly] public int ID;
    [ReadOnly] public string CurrentStateName;

    
    protected StateMachine StateMachine;
    protected void Awake()
    {
        // Create the State Machine.
        StateMachine = new StateMachine();
        ID = StateMachine.ID;
        StateMachine.OnStateChanged += StateChanged;
    }
    private void StateChanged(IState newState) => CurrentStateName = newState != null ? newState.GetType().ToString() : "Null";


    // Call the State Machine's tick method.
    private void Update() => StateMachine.Tick();
}
