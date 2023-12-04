using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private IState _currentState;
    private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
    private List<Transition> _currentTransitions = new List<Transition>();
    private List<Transition> _anyTransitions = new List<Transition>();
    private static List<Transition> EmptyTransitions = new List<Transition>(capacity: 0);

    public int ID;

    public StateMachine() => ID = UnityEngine.Random.Range(0, 10000);


    public void Tick()
    {
        // Check for transitions.
        Transition transition = GetTransition();
        if (transition != null)
            SetState(transition.To);

        // Call Tick() on the state.
        _currentState?.Tick();
    }

    public void SetState(IState state)
    {
        // Prevents transitioning back to the same state.
        if (_currentState == state)
            return;

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
    public void AddAnyTransition(IState state, Func<bool> predicate)
    {
        _anyTransitions.Add(new Transition(state, predicate));
    }


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

    private Transition GetTransition()
    {
        // Transitions from Any State.
        foreach (Transition transition in _anyTransitions)
            if (transition.Condition() && (transition.To != _currentState))
                return transition;

        // Transitions from Current State.
        foreach (Transition transition in _currentTransitions)
            if (transition.Condition())
                return transition;

        // No available transitions.
        return null;
    }
}
