using System.Collections.Generic;

/*
 * A Hierarchical State Machine based off the one created by Inspiaaa.
 * Link to Original Repo: https://github.com/Inspiaaa/UnityHFSM/blob/master/README.md
*/


namespace UnityHFSM
{
    /// <summary> A Finite State Machine that can also be used as a state of a parent state machine to create a Hierarchical State Machine.</summary>
    public class StateMachine<TOwnID, TStateID, TEvent> :
        StateBase<TOwnID>,
        ITriggerable<TEvent>,
        IStateMachine,
        IActionable<TEvent>
    {
        #region Bundles
        /// <summary> A bundle of states with their outgoing transitions and trigger transitions.
        ///     Useful as we now only need to do a single Dictionary lookup for these three items, rather than separate lookups.</summary>
        private class StateBundle
        {
            // By default, these fuelds are all null and will only be assigned a value when you need them.
            public StateBase<TStateID> State;
            public List<TransitionBase<TStateID>> Transitions;
            public Dictionary<TEvent, List<TransitionBase<TStateID>>> TriggerToTransitions;


            public void AddTransition(TransitionBase<TStateID> transition)
            {
                // If the Transitions list does not exist, create it.
                Transitions = Transitions ?? new List<TransitionBase<TStateID>>();

                // Add this transition to the Transitions list.
                Transitions.Add(transition);
            }

            public void AddTriggerTransition(TEvent trigger, TransitionBase<TStateID> transition)
            {
                // If the TriggerToTransitions dictionary does not exist, create it.
                TriggerToTransitions = TriggerToTransitions ?? new Dictionary<TEvent, List<TransitionBase<TStateID>>>();

                List<TransitionBase<TStateID>> transitionsOfTrigger;

                // Get the value of the transitions of this trigger.
                if (!TriggerToTransitions.TryGetValue(trigger, out transitionsOfTrigger))
                {
                    // If there are none, create a new list and add the new values to the dictionary.
                    transitionsOfTrigger = new List<TransitionBase<TStateID>>();
                    TriggerToTransitions.Add(trigger, transitionsOfTrigger);
                }

                // Add this transition to the list of transitions from this trigger.
                transitionsOfTrigger.Add(transition);
            }
        }

        private struct PendingTransition
        {
            public TStateID TargetState;
            public bool IsExitTransition;

            // An optional value (May be null), used for callbacks when the transition succeeds.
            public ITransitionListener Listener;

            // As structs are a value type and therefore not nullable, this field is required to see if the pending transition has been set yet.
            public bool IsPending;


            public static PendingTransition CreateForExit(ITransitionListener listener = null) => new PendingTransition
            {
                TargetState = default,
                IsExitTransition = true,
                Listener = listener,
                IsPending = true,
            };

            public static PendingTransition CreateForState(TStateID target, ITransitionListener listener = null) => new PendingTransition
            {
                TargetState = target,
                IsExitTransition = false,
                Listener = listener,
                IsPending = false,
            };
        }
        #endregion

        // A cached empty list of transitions (For easier readability when assigning them).
        private static readonly List<TransitionBase<TStateID>> _noTransitions = new List<TransitionBase<TStateID>>(0);
        private static readonly Dictionary<TEvent, List<TransitionBase<TStateID>>> _noTriggerTransitions = new Dictionary<TEvent, List<TransitionBase<TStateID>>>(0);


        private (TStateID state, bool hasState) _startState = (default, false);
        private PendingTransition _pendingTransition = default;


        // Central Storage of States.
        private Dictionary<TStateID, StateBundle> _stateBundlesByName = new Dictionary<TStateID, StateBundle>();

        private StateBase<TStateID> _activeState = null;
        private List<TransitionBase<TStateID>> _activeTransitions = _noTransitions;
        private Dictionary<TEvent, List<TransitionBase<TStateID>>> _activeTriggerTransitions = _noTriggerTransitions;

        private List<TransitionBase<TStateID>> _transitionsFromAny = new List<TransitionBase<TStateID>>();
        private Dictionary<TEvent, List<TransitionBase<TStateID>>> _triggerTransitionsFromAny = new Dictionary<TEvent, List<TransitionBase<TStateID>>>();


        // Getters/Setters.
        public StateBase<TStateID> ActiveState
        {
            get
            {
                EnsureIsInitialisedFor("Tring to get the active state");
                return _activeState;
            }
        }
        public TStateID ActiveStateName => ActiveState.Name;

        public IStateMachine ParentFSM => FSM;

        private bool _isRootFSM => FSM == null;

        public bool HasPendingTransition => _pendingTransition.IsPending;



        /// <summary> Initialises a new instance of a StateMachine class.</summary>
        /// <param name="needsExitTime"> (Only for Hierarchical State Machines):
        ///     Determines whether the State Machine as a state of a parent state machine is allowed to instantly exit on
        ///     a transition (False), or if it should wait until an explicit exit transition occurs (True).</param>
        public StateMachine(bool needsExitTime = false, bool isGhostState = false) : base (needsExitTime, needsExitTime)
        {

        }

        /// <summary> Throws an exception if the State Machine is not initialised yet.</summary>
        /// <param name="context"> String message for which action the FSM should be initialised for.</param>
        private void EnsureIsInitialisedFor(string context)
        {
            if (_activeState == null)
                throw UnityHFSM.Exceptions.Common.NotInitialised(context);
        }


        /// <summary> Notifies the State Machine that the state can clearnly exit, allowing us to execute any pending transitions.</summary>
        public void StateCanExit()
        {
            // If there is no pending transition, return.
            if (!_pendingTransition.IsPending)
                return;


            ITransitionListener listener = _pendingTransition.Listener;
            if (_pendingTransition.IsExitTransition)
            {
                // This is a vertical transition (Up the Hierarchy).
                _pendingTransition = default;

                listener?.BeforeTransition();
                PerformVerticalTransition();
                listener?.AfterTransition();
            }
            else
            {
                // This is a horizontal transition (Across the Hierarchy).
                TStateID state = _pendingTransition.TargetState;

                // When the pending state is a Ghost State, ChangeState() will have to try all outgoing transitions, which may override the pendingState.
                // That is why we are clearing the pending state beforehand, and not afterwards, as that would override the new valid pending state.
                _pendingTransition = default;
                ChangeState(state, listener);
            }
        }


        /// <summary> Instantly changes to the target state.</summary>
        private void ChangeState(TStateID name, ITransitionListener listener = null)
        {
            listener?.BeforeTransition();
            _activeState?.OnExit();

            StateBundle bundle;

            // If we are trying to transition to a state that does not exist, throw an exception.
            if (!_stateBundlesByName.TryGetValue(name, out bundle) || bundle.State == null)
            {
                //throw UnityHFSM.Exceptions.Common.StateNotFound(name.ToString(), context: "Switching States");
            }


            // Set the active transitions.
            _activeTransitions = bundle.Transitions ?? _noTransitions;
            _activeTriggerTransitions = bundle.TriggerToTransitions ?? _noTriggerTransitions;

            // Set our active state and inform it that it is now active.
            _activeState = bundle.State;
            _activeState.OnEnter();

            // Inform all active transitions that we have entered this state.
            for (int i = 0; i < _activeTransitions.Count; i++)
            {
                _activeTransitions[i].OnEnter();
            }

            // Inform all active trigger transitions that we have entered this state.
            foreach (List<TransitionBase<TStateID>> transitions in _activeTriggerTransitions.Values)
            {
                for (int i = 0; i < transitions.Count; i++)
                {
                    transitions[i].OnEnter();
                }
            }

            
            // Alert the listener (If it exists) that the transition has completed.
            listener?.AfterTransition();

            // If the active state is a Ghost State, instantly attempt to transition out of it.
            if (_activeState.IsGhostState)
            {
                TryAllDirectTransitions();
            }
        }


        /// <summary> Signals to the parent FSM that this FSM can exit, which allows the parent FSM to transition to the next state.</summary>
        private void PerformVerticalTransition()
        {
            FSM?.StateCanExit();
        }


        /// <summary> Requests a State Change, respecting the "Needs Exit Time" property of the active state.</summary>
        /// <param name="name"> The name/identifier of the target state.</param>
        /// <param name="forceInstantly"> Overrides the NeedsExitTime of the active state if true, forcing an immediate state change.</param>
        /// <param name="listener"> An optional object that recieves callbacks before and after the transition.</param>
        public void RequestStateChange(TStateID name, bool forceInstantly = false, ITransitionListener listener = null)
        {
            // If we don't need to wait for the state to exit, then instantly transition.
            if (!_activeState.NeedsExitTime || forceInstantly)
            {
                _pendingTransition = default;
                ChangeState(name, listener);
            }
            else
            {
                _pendingTransition = PendingTransition.CreateForState(name, listener);
                _activeState.OnExitRequest();
                // If the state can exit, the activeState would call state.FSM.StateCanExit(), which would in turn call FSM.ChangeState().
            }
        }


        /// <summary> Requests a vertical transition, allowing the state machine to allow the parent FSM to transition to the next state, respecting the NeedsExitTime of the currently active state.</summary>
        /// <param name="forceInstantly"> Overrides the NeedsExitTime of the active state if true, forcing an immediate state change.</param>
        /// <param name="listener"> An optional object that recieves callbacks before and after the transition.</param>
        public void RequestExit(bool forceInstantly = false, ITransitionListener listener = null)
        {
            // If we don't need to wait for the state to exit, then instantly transition.
            if (!_activeState.NeedsExitTime || forceInstantly)
            {
                _pendingTransition = default;
                listener?.BeforeTransition();
                PerformVerticalTransition();
                listener?.AfterTransition();
            }
            else
            {
                _pendingTransition = PendingTransition.CreateForExit(listener);
                _activeState.OnExitRequest();
            }
        }


        /// <summary> Checks if a transition can take place, and if this is the case, transitions to the 'To' state and returns true. Otherwise, it returns false.</summary>
        private bool TryTransition(TransitionBase<TStateID> transition)
        {
            if (transition.IsExitTransition)
            {
                // If the parent FSM is null, or has a pending transition, or we shouldn't transition, then don't transition.
                if (FSM == null || !FSM.HasPendingTransition || !transition.ShouldTransition())
                    return false;

                // Otherwise, request an exit transition.
                RequestExit(transition.ForceInstantly, transition as ITransitionListener);
                return true;
            }
            else
            {
                // If the transition says we shouldn't transition, then don't transition.
                if (!transition.ShouldTransition())
                    return false;

                // Otherwise, request a State Change.
                RequestStateChange(transition.To, transition.ForceInstantly, transition as ITransitionListener);
                return true;
            }
        }


        /// <summary> Tries all the "Global Transitions" (Transitions that can occur from any state).</summary>
        /// <returns> Returns true if a transition occured.</returns>
        private bool TryAllGlobalTransitions()
        {
            for (int i = 0; i < _transitionsFromAny.Count; i++)
            {
                TransitionBase<TStateID> transition = _transitionsFromAny[i];


                // Don't transition to the "To" state if that state is already the active state.
                if (EqualityComparer<TStateID>.Default.Equals(transition.To, _activeState.Name))
                    continue;


                // Attempt the transition.
                if (TryTransition(transition))
                    return true;
            }

            // We didn't successfully transition.
            return false;
        }


        /// <summary> Tries the "normal" transitions that transition from one specific state to another.</summary>
        /// <returns> Returns true if a transition occured.</returns>
        private bool TryAllDirectTransitions()
        {
            for (int i = 0; i < _activeTransitions.Count; i++)
            {
                TransitionBase<TStateID> transition = _activeTransitions[i];

                // Attempt the transition.
                if (TryTransition(transition))
                    return true;
            }

            // We didn't successfully transition.
            return false;
        }


        /// <summary> Calls OnEnter if it is the root state machine, therefore initialising the state machine.</summary>
        public override void Init()
        {
            if (!_isRootFSM)
                return;

            OnEnter();
        }

        /// <summary> Initialises the State Machine and must be called before OnLogic is called. It sets the activeState to the selected startState.</summary>
        public override void OnEnter()
        {
            // If the starting state doesn't contain a state, then throw an exception.
            if (!_startState.hasState)
            {
                throw UnityHFSM.Exceptions.Common.MissingStartState(context: "Running OnEnter of the state machine");
            }

            // Clear any previous pending transitions from the last run.
            _pendingTransition = default;


            ChangeState(_startState.state);

            // Tell all global transitions that we have entered our first state.
            for (int i = 0; i < _transitionsFromAny.Count; i++)
            {
                _transitionsFromAny[i].OnEnter();
            }

            foreach (List<TransitionBase<TStateID>> transitions in _triggerTransitionsFromAny.Values)
            {
                for (int i = 0; i < transitions.Count; i++)
                {
                    transitions[i].OnEnter();
                }
            }
        }


        /// <summary> Runs one Logic Step. It does at most one transition itself and calls teh active state's logic function (After a state transition, if one occured).</summary>
        public override void OnLogic()
        {
            // Throws an exception if the FSM is not yet initialised.
            EnsureIsInitialisedFor("Running OnLogic");

            if (TryAllGlobalTransitions())
                goto runOnLogic;

            if (TryAllDirectTransitions())
                goto runOnLogic;

            runOnLogic:
            _activeState?.OnLogic();
        }


        public override void OnExit()
        {
            if (_activeState != null)
            {
                _activeState.OnExit();

                // By setting _activeState to null, the state's OnExit method won't be caleld a second time when the state machine enters again.
                _activeState = null;
            }
        }

        public override void OnExitRequest()
        {
            if (_activeState.NeedsExitTime)
                _activeState.OnExitRequest();
        }


        /// <summary> Defines the entry point of the State Machine.</summary>
        /// <param name="name">The name/identifier of the start state.</param>
        public void SetStartState(TStateID name)
        {
            _startState = (name, true);
        }


        /// <summary> Gets the StateBundle belonging to the <c>name</c> state "slot" if it exists.
        ///     Otherwise, it will create a new StateBundle, that will be added to the Dictionary,
        ///     and return the newly created instance.</summary>
        private StateBundle GetOrCreateStateBundle(TStateID name)
        {
            StateBundle bundle;

            // If there is no state bundle containing this state, create one.
            if (!_stateBundlesByName.TryGetValue(name, out bundle))
            {
                bundle = new StateBundle();
                _stateBundlesByName.Add(name, bundle);
            }

            // Return the found/created bundle.
            return bundle;
        }


        /// <summary> Adds a new node/state to the State Machine.</summary>
        /// <param name="name"> The name/identifier of the new state.</param>
        /// <param name="state"> The new state instance (E.g. State, StateMachine, etc).</param>
        public void AddState(TStateID name, StateBase<TStateID> state)
        {
            state.FSM = this;
            state.Name = name;
            state.Init();

            StateBundle bundle = GetOrCreateStateBundle(name);
            bundle.State = state;

            // If we don't have a valid start state, set this state to the start state.
            if (_stateBundlesByName.Count == 1 && !_startState.hasState)
            {
                SetStartState(name);
            }
        }


        /// <summary> Initialises a transition (Sets its FSM attribute & then calls its Init method).</summary>
        private void InitTransition(TransitionBase<TStateID> transition)
        {
            transition.FSM = this;
            transition.Init();
        }


        #region Adding Transitions.
        /// <summary> Adds a new transition between two states.</summary>
        /// <param name="transition"> The transition instance.</param>
        public void AddTransition(TransitionBase<TStateID> transition)
        {
            // Initialise the transition.
            InitTransition(transition);

            // Bundle this transition with the associated state.
            StateBundle bundle = GetOrCreateStateBundle(transition.From);
            bundle.AddTransition(transition);
        }

        /// <summary> Adds a new transition that can happen from any possible state.</summary>
        /// <param name="transition"> The transition instance. The "From" field can be left empty, as it has no meaning in this context.</param>
        public void AddTransitionFromAny(TransitionBase<TStateID> transition)
        {
            // Initialise the transition.
            InitTransition(transition);

            // Add this transition to the list of global transitions.
            _transitionsFromAny.Add(transition);
        }

        /// <summary> Adds a new trigger transition between two states that is only checked when the specified trigger is activated. </summary>
        /// <param name="trigger"> The name/identifier of the trigger.</param>
        /// <param name="transition"> The transition instance (E.g. Transition, TransitionAfter, etc).</param>
        public void AddTriggerTransition(TEvent trigger, TransitionBase<TStateID> transition)
        {
            // Initialise the transition.
            InitTransition(transition);

            // Bundles this transition with the associated state as a trigger transition.
            StateBundle bundle = GetOrCreateStateBundle(transition.From);
            bundle.AddTriggerTransition(trigger, transition);
        }

        /// <summary> Adds a new trigger transition that can happen from any possible state, but is only checked when the specified trigger is activated.</summary>
        /// <param name="trigger"> The name/identifier of the trigger.</param>
        /// <param name="transition"> The transition instance. The "From" field can be left empty, as it has no meaning in this context.</param>
        public void AddTriggerTransitionFromAny(TEvent trigger, TransitionBase<TStateID> transition)
        {
            // Initialise the transition.
            InitTransition(transition);

            // Get the transitions of this trigger.
            List<TransitionBase<TStateID>> transitionsOfTrigger;
            if (!_triggerTransitionsFromAny.TryGetValue(trigger, out transitionsOfTrigger))
            {
                // If there are none, create a new list and add the new values to the dictionary. 
                transitionsOfTrigger = new List<TransitionBase<TStateID>>();
                _triggerTransitionsFromAny.Add(trigger, transitionsOfTrigger);
            }

            // Add this transition to the list of transitions from this trigger.
            transitionsOfTrigger.Add(transition);
        }


        /// <summary> Adds two transitions:
        ///     If the condition of the transition instance is true, it transitions from the "From" state to the "To" state.
        ///     Otherwise, it performs a transition in the opposite direction ("To" to "From").</summary>
        /// <remarks> Internally the same transition instance will be used for both transitions by wrapping it in a 'ReverseTransition'.
		///     For the reverse transition the afterTransition callback is called before the transition and the onTransition callback afterwards. 
        ///     If this is not desired then replicate the behaviour of the two way transitions by creating two separate transitions.
		/// </remarks>
        public void AddTwoWayTransition(TransitionBase<TStateID> transition)
        {
            // Initialise and add the forward transition.
            InitTransition(transition);
            AddTransition(transition);

            // Initialise and add the reverse transition.
            ReverseTransition<TStateID> reverse = new ReverseTransition<TStateID>(transition, false);
            InitTransition(reverse);
            AddTransition(reverse);
        }

        /// <summary> Adds two transitions that are only checked when a specified trigger is activated:
        ///     If the condition of the transition instance is true, it transitions from the "From" state to the "To" state.
        ///     Otherwise, it performs a transition in the opposite direction ("To" to "From").</summary>
        /// <remarks> Internally the same transition instance will be used for both transitions by wrapping it in a 'ReverseTransition'.
		///     For the reverse transition the afterTransition callback is called before the transition and the onTransition callback afterwards. 
        ///     If this is not desired then replicate the behaviour of the two way transitions by creating two separate transitions.
		/// </remarks>
        public void AddTwoWayTriggerTransition(TEvent trigger, TransitionBase<TStateID> transition)
        {
            // Initialise and add the forward transition.
            InitTransition(transition);
            AddTriggerTransition(trigger, transition);

            // Initialise and add the reverse transition.
            ReverseTransition<TStateID> reverse = new ReverseTransition<TStateID>(transition, false);
            InitTransition(reverse);
            AddTriggerTransition(trigger, reverse);
        }


        /// <summary> Adds a new exit transition from a state.
        ///     It represents an exit point that allows the FSM to exit and allows the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <param name="transition"> The transition instance. The "To" field can be left empty, as it has no meaning in this context.</param>
        public void AddExitTransition(TransitionBase<TStateID> transition)
        {
            transition.IsExitTransition = true;
            AddTransition(transition);
        }

        /// <summary> Adds a new exit transition that can happen from any possible state.
        ///     It represents an exit point that allows the FSM to exit and allows the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <param name="transition"> The transition instance. The "From" and "To" fields can be left empty, as they have no meaning in this context.</param>
        public void AddExitTransitionFromAny(TransitionBase<TStateID> transition)
        {
            transition.IsExitTransition = true;
            AddTransitionFromAny(transition);
        }

        /// <summary> Adds a new exit transition from a state that is only checked when the specified trigger is activated.
        ///     It represents an exit point that allows the FSM to exit and allows the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <param name="transition"> The transition instance. The "To" field can be left empty, as it has no meaning in this context.</param>
        public void AddExitTriggerTransition(TEvent trigger, TransitionBase<TStateID> transition)
        {
            transition.IsExitTransition = true;
            AddTriggerTransition(trigger, transition);
        }

        /// <summary> Adds a new exit transition that can happen from any possible state and is only checked when the specified trigger is activated.
        ///     It represents an exit point that allows the FSM to exit and allows the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <param name="transition"> The transition instance. The "From" and "To" fields can be left empty, as they have no meaning in this context.</param>
        public void AddExitTriggerTransitionFromAny(TEvent trigger, TransitionBase<TStateID> transition)
        {
            transition.IsExitTransition = true;
            AddTriggerTransitionFromAny(trigger, transition);
        }
#endregion



        /// <summary> Activates the specified trigger, checking all targeted trigger transitions to see whether a transition should occur.</summary>
        /// <param name="trigger"> The name/identifier of the trigger.</param>
        /// <returns> True when a transition occured, otherwise false.</returns>
        private bool TryTrigger(TEvent trigger)
        {
            // Ensure that the StateMachine is initialised.
            EnsureIsInitialisedFor("Checking for all trigger transitions of the active state");

            List<TransitionBase<TStateID>> triggerTransitions;

            // Attempt global trigger transitions.
            if (_triggerTransitionsFromAny.TryGetValue(trigger, out triggerTransitions))
            {
                for (int i = 0; i < triggerTransitions.Count; i++)
                {
                    TransitionBase<TStateID> transition = triggerTransitions[i];

                    // Ensure that we don't transition to the state we are currently in.
                    if (EqualityComparer<TStateID>.Default.Equals(transition.To, _activeState.Name))
                        continue;

                    // Attempt the transition.
                    if (TryTransition(transition))
                        return true;
                }
            }

            // Attempt local trigger transitions.
            if (_activeTriggerTransitions.TryGetValue(trigger, out triggerTransitions))
            {
                for (int i = 0; i < triggerTransitions.Count; i++)
                {
                    TransitionBase<TStateID> transition = triggerTransitions[i];

                    // Attempt the transition.
                    if (TryTransition(transition))
                        return true;
                }
            }

            // We didn't successfully transition.
            return false;
        }

        /// <summary> Activates the specified trigger in all active states of the hierarchy, checking all trigger transitions to see whether a transition should occur.</summary>
        /// <param name="trigger"> The name/identifier of the trigger.</param>
        public void Trigger(TEvent trigger)
        {
            // If a transition occurs, then the trigger should not be activated in the new/active state that the State Machine just switched to.
            if (TryTrigger(trigger))
                return;

            // Activate the trigger on the current state (If the state is triggerable).
            (_activeState as ITriggerable<TEvent>)?.Trigger(trigger);
        }

        /// <summary> Only activates the specified trigger locally in this State Machine.</summary>
        /// <param name="trigger"> The name/identifier of the trigger.</param>
        public void TriggerLocally(TEvent trigger)
        {
            TryTrigger(trigger);
        }


        /// <summary> Runs an action on the currently active state.</summary>
        /// <param name="trigger"> The name of the action.</param>
        public virtual void OnAction(TEvent trigger)
        {
            // Ensure that the StateMachine has been initialised.
            EnsureIsInitialisedFor("Running OnAction of the active state");

            // Call OnAction on the active state (If it is IActionable).
            (_activeState as IActionable<TEvent>)?.OnAction(trigger);
        }

        /// <summary> Runs an action on the currently active state and lets you pass one data parameter.</summary>
        /// <param name="trigger"> The name of the action.</param>
        /// <param name="data"> Any custom data for the parameter.</param>
        /// <typeparam name="TData"> Type of the data parameter. Should match the data type of the action that was added via AddAction<T>(...).</typeparam>
        public virtual void OnAction<TData>(TEvent trigger, TData data)
        {
            // Ensure that the StateMachine has been initialised.
            EnsureIsInitialisedFor("Running OnAction of the active state");

            // Call OnAction on the active state (If it is IActionable).
            (_activeState as IActionable<TEvent>)?.OnAction<TData>(trigger, data);
        }


        public StateBase<TStateID> GetState(TStateID name)
        {
            StateBundle bundle;

            // If there is no State Bundle associated with this name, or that bundle has no associated state, throw an exception.
            if (!_stateBundlesByName.TryGetValue(name, out bundle) || bundle.State == null)
            {
                throw UnityHFSM.Exceptions.Common.StateNotFound(name.ToString(), context: "Getting a State");
            }

            // Otherwise, return the associated state of the found bundle.
            return bundle.State;
        }

        // An indexer used for getting a nested State Machine.
        public StateMachine<string, string, string> this[TStateID name]
        {
            get
            {
                StateBase<TStateID> state = GetState(name);
                StateMachine<string, string, string> subFSM = state as StateMachine<string, string, string>;

                if (subFSM == null)
                {
                    throw UnityHFSM.Exceptions.Common.QuickIndexerMisusedForGettingState(name.ToString());
                }

                return subFSM;
            }
        }

        public override string GetActiveHierarchyPath()
        {
            if (_activeState == null)
            {
                // When the state machine is not active, then the active hierarchy path is empty.
                return "";
            }

            return $"{Name}/{_activeState.GetActiveHierarchyPath()}";
        }
    }


    #region Overloaded Classes
    // Overloaded classes to allow for an easier useage of the StateMachine for common cases.
    // (E.g. "new StateMachine()" instead of "new StateMachine<string, string, string>()").
    
    
    /// <inheritdoc/>
    public class StateMachine<TStateID, TEvent> : StateMachine<TStateID, TStateID, TEvent>
    {
        /// <inheritdoc/>
        public StateMachine(bool needsExitTime = false, bool isGhostState = false) : base(needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }

    /// <inheritdoc/>
    public class StateMachine<TStateID> : StateMachine<TStateID, TStateID, string>
    {
        /// <inheritdoc/>
        public StateMachine(bool needsExitTime = false, bool isGhostState = false) : base(needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }

    /// <inheritdoc/>
    public class StateMachine : StateMachine<string, string, string>
    {
        /// <inheritdoc/>
        public StateMachine(bool needsExitTime = false, bool isGhostState = false) : base(needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }
    #endregion
}