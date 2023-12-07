using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A class to handle the transition between states.</summary>
public class StateMachine
{
    private IState _currentState;
    private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
    private List<Transition> _currentTransitions = new List<Transition>();
    private List<Transition> _anyTransitions = new List<Transition>();
    private static List<Transition> EmptyTransitions = new List<Transition>(capacity: 0);

    public int ID; // An ID to represent this StateMachine (Used for Debugging).

    public StateMachine() => ID = UnityEngine.Random.Range(0, 10000);


    public void Tick()
    {
        // Check for valid transitions.
        Transition transition = GetTransition();
        
        // If there is a valid transition, change to that state.
        if (transition != null)
            SetState(transition.To);

        // Call the 'Tick' method on the current state.
        _currentState?.Tick();
    }

    /// <summary> Set the current state (Accessed outwith the StateMachine only for setting the Initial State).</summary>
    public void SetState(IState state)
    {
        // Prevents transitioning back to the same state.
        if (_currentState == state)
            return;

        // (Debug) Display the new state's name.
        Debug.Log(ID + " - New State: " + state);

        // Exit the current state.
        _currentState?.OnExit();
        _currentState = state;

        // Prepare the state's transitions.
        _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
        if (_currentTransitions == null)
            _currentTransitions = EmptyTransitions;

        // Initialize the state.
        _currentState.OnEnter();
    }

    /// <summary> Add a new transition between two states to this state machine.</summary>
    public void AddTransition(IState from, IState to, Func<bool> predicate)
    {
        // Try to get the current values of the transitions from the 'from' state.
        if (_transitions.TryGetValue(from.GetType(), out List<Transition> transitions) == false)
        {
            // If there are none (Couldn't get the value), create a new list to hold them.
            transitions = new List<Transition>();
            _transitions[from.GetType()] = transitions;
        }

        // Add the new transition.
        transitions.Add(new Transition(to, predicate));
    }
    /// <summary> Add a transition to occur from any state to this state machine.</summary>
    public void AddAnyTransition(IState state, Func<bool> predicate) => _anyTransitions.Add(new Transition(state, predicate));
    

    /// <summary> A class to represent a transition between states.</summary>
    private class Transition
    {
        public Func<bool> Condition { get; }
        public IState To { get; }
        
        public Transition(IState to, Func<bool> condition)
        {
            this.Condition = condition;
            this.To = to;
        }
    }

    /// <summary> Get a valid transition if one exits</summary>
    private Transition GetTransition()
    {
        // Check transitions that can occur from any State.
        foreach (Transition transition in _anyTransitions)
            if (transition.Condition() && (transition.To != _currentState))
                return transition;

        // Check transitions from the Current State.
        foreach (Transition transition in _currentTransitions)
            if (transition.Condition())
                return transition;

        // There are no valid/available transitions.
        return null;
    }
}
