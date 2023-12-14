using System;

namespace UnityHFSM
{
    /// <summary> The "normal" state class that can run code on Enter, on Logic, and On Exit, while also handling the timing of the next state transition.</summary>
    public class State<TStateID, TEvent> : ActionState<TStateID, TEvent>
    {
        private Action<State<TStateID, TEvent>> _onEnter;
        private Action<State<TStateID, TEvent>> _onLogic;
        private Action<State<TStateID, TEvent>> _onExit;
        private Func<State<TStateID, TEvent>, bool> _canExit;


        public ITimer Timer;


        /// <summary> Initialises a new instance of the State Class.</summary>
        /// <param name="onEnter"> A function that is called when the State Machine enters this state.</param>
        /// <param name="onLogic"> A function that is called by the Logic function of the State Machine if this state is active.</param>
        /// <param name="onExit"> A function that is called when the State Machine enters this state.</param>
        /// <param name="canExit"> (Only if needsExitTime is true):
        ///     A function that determines if the state is ready to exit (True) or not (False).
        ///     It is called in OnExitRequest and on each logic step when a transition is pending.</param>
        /// <param name="needsExitTime"> Determines if a state is allowed to instantly exit on a transition (False),
        ///     or if the State Machine should wait for until the State is ready for a state change (True).</param>
        public State(
            Action<State<TStateID, TEvent>> onEnter = null,
            Action<State<TStateID, TEvent>> onLogic = null,
            Action<State<TStateID, TEvent>> onExit = null,
            Func<State<TStateID, TEvent>, bool> canExit = null,
            bool needsExitTime = false, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._onEnter = onEnter;
            this._onLogic = onLogic;
            this._onExit = onExit;
            this._canExit = canExit;

            this.Timer = new Timer();
        }


        public override void OnEnter()
        {
            Timer.Reset();

            _onEnter?.Invoke(this);
        }
        public override void OnLogic()
        {
            // If we can exit this state, and the FSM is awaiting a transition, tell the FSM that we can transition.
            if (NeedsExitTime && _canExit != null && FSM.HasPendingTransition && _canExit(this))
                FSM.StateCanExit();
            
            _onLogic?.Invoke(this);
        }
        public override void OnExit()
        {
            _onExit?.Invoke(this);
        }

        public override void OnExitRequest()
        {
            if (_canExit != null && _canExit(this))
                FSM.StateCanExit();
        }
    }

    #region Overloaded Classes
    // Overloaded Classes allow for an easier useage of the class for common cases.

    /// <inheritdoc/>
    public class State<TStateID> : State<TStateID, string>
    {
        /// <inheritdoc/>
        public State(
            Action<State<TStateID, string>> onEnter = null,
            Action<State<TStateID, string>> onLogic = null,
            Action<State<TStateID, string>> onExit = null,
            Func<State<TStateID, string>, bool> canExit = null,
            bool needsExitTime = false, bool isGhostState = false) : base( onEnter, onLogic, onExit, canExit, needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }

    /// <inheritdoc/>
    public class State : State<string, string>
    {
        /// <inheritdoc/>
        public State(
            Action<State<string, string>> onEnter = null,
            Action<State<string, string>> onLogic = null,
            Action<State<string, string>> onExit = null,
            Func<State<string, string>, bool> canExit = null,
            bool needsExitTime = false, bool isGhostState = false) : base(onEnter, onLogic, onExit, canExit, needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }
    #endregion
}