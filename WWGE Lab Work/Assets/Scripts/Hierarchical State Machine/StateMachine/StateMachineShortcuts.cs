using System;

namespace UnityHFSM
{
    /*
     "Shortcut" Methods.
        - These are meant to reduce the biolerplate code required by the user for simple states and transitions.
        - They do this by setting a new State/Transition instance in the background and then setting the desired fields.
        - They can also optimise certain cases for you by chosing teh best type, such as a StateBase for an empty state instead of a State instance.
     */
    public static class StateMachineShortcuts
    {
        /// <summary> A shortcut method for adding a regular state.</summary>
        /// <remarks> It creates a new State() instance under the hood (See State for more information).
        ///     For empty states with no logic it creates a new StateBase for optimal performance.</remarks>
        /// <inheritdoc cref="State{TStateID, TEvent}(
        ///     Action{State{TStateID, TEvent}},
        ///     Action{State{TStateID, TEvent}},
        ///     Action{State{TStateID, TEvent}},
        ///     Func{State{TStateID, TEvent}, bool},
        ///     bool,
        ///     bool
        /// )"/>
        public static void AddState<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TStateID name,
            Action<State<TStateID, TEvent>> onEnter = null,
            Action<State<TStateID, TEvent>> onLogic = null,
            Action<State<TStateID, TEvent>> onExit = null,
            Func<State<TStateID, TEvent>, bool> canExit = null,
            bool needsExitTime = false,
            bool isGhostState = false)
        {
            // Optimise for empty states.
            if (onEnter == null && onLogic == null && onExit == null && canExit == null)
            {
                fsm.AddState(name, new StateBase<TStateID>(needsExitTime, isGhostState));
                return;
            }

            fsm.AddState(
                name,
                new State<TStateID, TEvent>(
                    onEnter,
                    onLogic,
                    onExit,
                    canExit,
                    needsExitTime: needsExitTime,
                    isGhostState: isGhostState
                    )
                );
        }


        /// <summary> Creates the most efficient transition type possible for the given parameters.
        ///     It creates a Transition instance when a condition or transition callbacks are specified,
        ///     otherwise it returns a TransitionBase.</summary>
        private static TransitionBase<TStateID> CreateOptimisedTransition<TStateID>(
            TStateID from,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            // Optimise for empty transitions.
            if (condition == null && onTransition == null && afterTransition == null)
            {
                return new TransitionBase<TStateID>(from, to, forceInstantly);
            }

            return new Transition<TStateID>(
                from,
                to,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                );
        }


        #region Standard Transitions
        /// <summary> Shortcut method for adding a regular transition.
        ///     It creates a new Transition() under the hood (See Transition for more information).</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddTransition<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TStateID from,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddTransition(CreateOptimisedTransition(
                from,
                to,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }

        /// <summary> Shortcut method for adding a regular transition that can happen from any state.
        ///     It creates a new Transition() under the hood (See Transition for more information).</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
        ///     Action{Transition{TStateID}},
        ///     Action{Transition{TStateID}},
        ///     bool
        /// )"/>
        public static void AddTransitionFromAny<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddTransitionFromAny(CreateOptimisedTransition(
                default,
                to,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }

        /// <summary> Shortcut method for adding a new trigger transition between two states that is only checked
        ///     when the specified trigger is activated.
        ///     It creates a new Transition() under the hood (See Transition for more information).</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddTriggerTransition<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TEvent trigger,
            TStateID from,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddTriggerTransition(trigger, CreateOptimisedTransition(
                from,
                to,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }

        /// <summary> Shortcut method for adding a new trigger transition that can happen from any possible state,
        ///     but is only checked when the specified trigger is activated.
        ///     It creates a new Transition() under the hood (See Transition for more information).</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddTriggerTransitionFromAny<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TEvent trigger,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddTriggerTransitionFromAny(trigger, CreateOptimisedTransition(
                default,
                to,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }


        /// <summary> Shortcut method for adding two transitions:
        ///     If the condition function is true, the FSM transition from the "From" state to the "To" state.
        ///     Otherwise, it performs a transition in the opposite direction ("To" to "From").</summary>
        /// <remarks> For the reverse transition the afterTransition callback is called before the transition and the onTransition callback afterwards.
        ///     If this is not desired then replicate the behaviour of the two way transitions by creating two separate transitions.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddTwoWayTransition<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TStateID from,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddTwoWayTransition(new Transition<TStateID>(
                from,
                to,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }

        /// <summary> Shortcut method for adding two transitions that are only checked when the specified trigger is activated:
        ///     If the condition function is true, the FSM transition from the "From" state to the "To" state.
        ///     Otherwise, it performs a transition in the opposite direction ("To" to "From").</summary>
        /// <remarks> For the reverse transition the afterTransition callback is called before the transition and the onTransition callback afterwards.
        ///     If this is not desired then replicate the behaviour of the two way transitions by creating two separate transitions.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddTwoWayTriggerTransition<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TEvent trigger,
            TStateID from,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddTwoWayTriggerTransition(trigger, new Transition<TStateID>(
                from,
                to,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }
        #endregion


        #region Exit Transitions
        /// <summary> A shortcut method for adding a new exit transition from a state.
        ///     It represents an exit point that allows the FSM to exit and the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
        /// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddExitTransition<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TStateID from,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddExitTransition(CreateOptimisedTransition(
                from,
                default,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }

        /// <summary> A shortcut method for adding a new exit transition that can happen from any state.
        ///     It represents an exit point that allows the FSM to exit and the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddExitTransitionFromAny<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddExitTransitionFromAny(CreateOptimisedTransition(
                default,
                default,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }

        /// <summary> A shortcut method for adding a new exit transition from a state that is only checked when the specified trigger is activated.
        ///     It represents an exit point that allows the FSM to exit and the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddExitTriggerTransition<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TEvent trigger,
            TStateID from,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddExitTriggerTransition(trigger, CreateOptimisedTransition(
                from,
                default,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }

        /// <summary> A shortcut method for adding a new exit transition that can happen from any possible state and is only checked when the specified trigger is activated.
        ///     It represents an exit point that allows the FSM to exit and the parent FSM to continue to the next state.
        ///     It is only checked if the parent FSM has a pending transition.</summary>
        /// <remarks> When no condition or callbacks are required, it creates a TransitionBase for optimal performance,
        ///     otherwise it creates a Transition object.</remarks>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool},
		/// 	Action{Transition{TStateID}},
        /// 	Action{Transition{TStateID}},
        /// 	bool
        /// )"/>
        public static void AddExitTriggerTransitionFromAny<TOwnID, TStateID, TEvent>(
            this StateMachine<TOwnID, TStateID, TEvent> fsm,
            TEvent trigger,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
            bool forceInstantly = false)
        {
            fsm.AddExitTriggerTransitionFromAny(trigger, CreateOptimisedTransition(
                default,
                default,
                condition,
                onTransition: onTransition,
                afterTransition: afterTransition,
                forceInstantly: forceInstantly
                ));
        }
#endregion
    }
}