using System;

namespace UnityHFSM
{
    /// <summary> A class that allows you to run additional functions (Companion Code) before and after the wrapped state's code.
    ///     It does not interfere with the wrapped state's timing/needsExitTime/etc behaviour.</summary>
    public class StateWrapper<TStateID, TEvent>
    {
        public class WrappedState : StateBase<TStateID>, ITriggerable<TEvent>, IActionable<TEvent>
        {
            // Companion Code Functions.
            private Action<StateBase<TStateID>>
                _beforeOnEnter, _afterOnEnter,
                _beforeOnLogic, _afterOnLogic,
                _beforeOnExit, _afterOnExit;

            // The wrapped state.
            private StateBase<TStateID> _state;



            public WrappedState(
                StateBase<TStateID> state,
                
                Action<StateBase<TStateID>> beforeOnEnter = null,
                Action<StateBase<TStateID>> afterOnEnter = null,
                
                Action<StateBase<TStateID>> beforeOnLogic = null,
                Action<StateBase<TStateID>> afterOnLogic = null,

                Action<StateBase<TStateID>> beforeOnExit = null,
                Action<StateBase<TStateID>> afterOnExit = null) : base(state.NeedsExitTime, state.IsGhostState)
            {
                this._state = state;


                this._beforeOnEnter = beforeOnEnter;
                this._afterOnEnter = afterOnEnter;

                this._beforeOnLogic = beforeOnLogic;
                this._afterOnLogic = afterOnLogic;

                this._beforeOnExit = beforeOnExit;
                this._afterOnExit = afterOnExit;
            }


            public override void Init()
            {
                // Override the wrapped state's default values.
                _state.Name = Name;
                _state.FSM = FSM;

                // Initialise the wrapped state.
                _state.Init();
            }

            public override void OnEnter()
            {
                _beforeOnEnter?.Invoke(this);
                _state.OnEnter();
                _afterOnEnter?.Invoke(this);
            }
            public override void OnLogic()
            {
                _beforeOnLogic?.Invoke(this);
                _state.OnLogic();
                _afterOnLogic?.Invoke(this);
            }
            public override void OnExit()
            {
                _beforeOnExit?.Invoke(this);
                _state.OnExit();
                _afterOnExit?.Invoke(this);
            }

            public override void OnExitRequest() => _state.OnExitRequest();
            


            public void Trigger(TEvent trigger) => (_state as ITriggerable<TEvent>)?.Trigger(trigger);
            public void OnAction(TEvent trigger) => (_state as IActionable<TEvent>)?.OnAction(trigger);
            public void OnAction<TData>(TEvent trigger, TData data) => (_state as IActionable<TEvent>)?.OnAction<TData>(trigger, data);
        }

        // Companion Code Functions.
        private Action<StateBase<TStateID>>
            _beforeOnEnter, _afterOnEnter,
            _beforeOnLogic, _afterOnLogic,
            _beforeOnExit, _afterOnExit;


        /// <summary> Initialises a new instance of the StateWrapper class.</summary>
        public StateWrapper(
            Action<StateBase<TStateID>> beforeOnEnter = null,
            Action<StateBase<TStateID>> afterOnEnter = null,

            Action<StateBase<TStateID>> beforeOnLogic = null,
            Action<StateBase<TStateID>> afterOnLogic = null,

            Action<StateBase<TStateID>> beforeOnExit = null,
            Action<StateBase<TStateID>> afterOnExit = null)
        {
            this._beforeOnEnter = beforeOnEnter;
            this._afterOnEnter = afterOnEnter;

            this._beforeOnLogic = beforeOnLogic;
            this._afterOnLogic = afterOnLogic;

            this._beforeOnExit = beforeOnExit;
            this._afterOnExit = afterOnExit;
        }


        public WrappedState Wrap(StateBase<TStateID> state) => new WrappedState(state, _beforeOnEnter, _afterOnEnter, _beforeOnLogic, _afterOnLogic, _beforeOnExit, _afterOnExit);
    }


    #region Overloaded Classes
    // Overloaded Classes allow for an easier useage of the class for common cases.

    /// <inheritdoc/>
    public class StateWrapper : StateWrapper<string, string>
    {
        public StateWrapper(
            Action<StateBase<string>> beforeOnEnter = null,
            Action<StateBase<string>> afterOnEnter = null,

            Action<StateBase<string>> beforeOnLogic = null,
            Action<StateBase<string>> afterOnLogic = null,

            Action<StateBase<string>> beforeOnExit = null,
            Action<StateBase<string>> afterOnExit = null
            ) : base (beforeOnEnter, afterOnEnter, beforeOnLogic, afterOnLogic, beforeOnExit, afterOnExit)
        {

        }
    }
    #endregion
}