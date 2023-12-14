using System;
using System.Collections.Generic;

namespace UnityHFSM
{
    /// <summary> Base class of states that import custom actions.</summary>
    /// <inheritdoc/>
    public class ActionState<TStateID, TEvent> : StateBase<TStateID>, IActionable<TEvent>
    {
        // Lazy initilisation.
        private ActionStorage<TEvent> _actionStorage;

        public ActionState(bool needsExitTime, bool isGhostState = false) : base(needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }


        /// <summary> Adds an action that can be called with OnAction().
        ///     Actions are like the built-in events OnEnter/OnLogic/etc, but are defined by the user.</summary>
        /// <param name="trigger"> The name of the action.</param>
        /// <param name="action"> The function that should be called when the action is run.</param>
        /// <returns> Itself to allow for a fluent interface.</returns>
        public ActionState<TStateID, TEvent> AddAction(TEvent trigger, Action action)
        {
            // If we have not initialised _actionStorage, initialise it.
            _actionStorage = _actionStorage ?? new ActionStorage<TEvent>();

            // Add the action to the actionStorage.
            _actionStorage.AddAction(trigger, action);


            return this;
        }

        /// <summary> Adds an action that can be called with OnAction<T>().
        ///     This overload allows you to run a function that takes one data parameter.
        ///     Actions are like the built-in events OnEnter/OnLogic/etc, but are defined by the user.</summary>
        /// <param name="trigger"> The name of the action.</param>
        /// <param name="action"> The function that should be called when the action is run.</param>
        /// <typeparam name="TData"> The data type of the parameter of the function.</typeparam>
        /// <returns> Itself to allow for a fluent interface.</returns>
        public ActionState<TStateID, TEvent> AddAction<TData>(TEvent trigger, Action<TData> action)
        {
            // If we have not initialised _actionStorage, initialise it.
            _actionStorage = _actionStorage ?? new ActionStorage<TEvent>();

            // Add the action to the actionStorage.
            _actionStorage.AddAction(trigger, action);


            return this;
        }


        /// <summary> Runs an action with the given name.
        ///     If the action is not defined/hasn't been added, nothing will happen.</summary>
        /// <param name="trigger"> The name of the action.</param>
        public void OnAction(TEvent trigger) => _actionStorage?.RunAction(trigger);

        /// <summary> Runs an action with the given name and lets you pass in one parameter to the action function.
        ///     If the action is not defined/hasn't been added, nothing will happen.</summary>
        /// <param name="trigger"> The name of the action.</param>
        /// <param name="data"> Data to pass as the first parameter to the action.</param>
        /// <typeparam name="TData"> The type of the data parameter.</typeparam>
        public void OnAction<TData>(TEvent trigger, TData data) => _actionStorage?.RunAction<TData>(trigger, data);
    }


    #region Overloaded Classes
    // Overloaded Classes allow for an easier useage of the class for common cases.
    
    /// <inheritdoc/>
    public class ActionState<TStateID> : ActionState<TStateID, string>
    {
        /// <inheritdoc/>
        public ActionState(bool needsExitTime, bool isGhostState = false) : base (needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }

    /// <inheritdoc/>
    public class ActionState : ActionState<string, string>
    {
        /// <inheritdoc/>
        public ActionState(bool needsExitTime, bool isGhostState = false) : base(needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }
    #endregion
}