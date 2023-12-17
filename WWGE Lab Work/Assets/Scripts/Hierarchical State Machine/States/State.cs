using System;

namespace UnityHFSM
{
    /// <summary> The "normal" state class that can run code on Enter, on Logic, and On Exit, while also handling the timing of the next state transition.</summary>
    public abstract class State<TEvent> : ActionState<TEvent>
    {
        private Action<State<TEvent>> _onEnter;
        private Action<State<TEvent>> _onLogic;
        private Action<State<TEvent>> _onExit;
        private Func<State<TEvent>, bool> _canExit;


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
            Action<State<TEvent>> onEnter = null,
            Action<State<TEvent>> onLogic = null,
            Action<State<TEvent>> onExit = null,
            Func<State<TEvent>, bool> canExit = null,
            bool needsExitTime = false, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._onEnter = onEnter;
            this._onLogic = onLogic;
            this._onExit = onExit;
            this._canExit = canExit;

            this.Timer = new Timer();
        }


        //public override void Init() { }
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
    public abstract class State : State<string>
    {
        /// <inheritdoc/>
        public State(
            Action<State<string>> onEnter = null,
            Action<State<string>> onLogic = null,
            Action<State<string>> onExit = null,
            Func<State<string>, bool> canExit = null,
            bool needsExitTime = false, bool isGhostState = false) : base(onEnter, onLogic, onExit, canExit, needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }
    #endregion
}