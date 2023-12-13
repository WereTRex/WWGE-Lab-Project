namespace UnityHFSM
{
    /// <summary> The base class of all States.</summary>
    public class StateBase<TStateID>
    {
        public bool NeedsExitTime;
        public bool IsGhostState;
        public TStateID Name;

        public IStateMachine FSM;

        /// <summary> Initialises a new instance of the StateBase class.</summary>
        /// <param name="needsExitTime">Determines if the state is allowed to instantly exit on a transition (False), or if the
        ///     FSM should wait until the state is ready to change (True).</param>
        /// <param name="isGhostState">If true, this state becomes a Ghost State, which the FSM doesn't want to stay in,
        ///     and will test all outgoing transitions instantly, as opposed to waiting for the next OnLogic call.</param>
        public StateBase(bool needsExitTime, bool isGhostState = false)
        {
            this.NeedsExitTime = needsExitTime;
            this.IsGhostState = isGhostState;
        }


        /// <summary> Called to initialise the state, after values like 'Name' and 'FSM' have been set.</summary>
        public virtual void Init()
        {

        }

        /// <summary> Called when the State Machine transitions into this state (Enters this state).</summary>
        public virtual void OnEnter()
        {

        }

        /// <summary> Called while this state is active (Typically on the Update frame).</summary>
        public virtual void OnLogic()
        {

        }

        /// <summary> Called when the State Machine transitions from this state (Exits this state).</summary>
        public virtual void OnExit()
        {

        }


        /// <summary> (Only called if NeedsExitTime is true): Called when a State Transition from this state should happen.
        ///     If it can happen, it should call FSM.StateCanExit()
        ///     If it cannot exit right not, it should call FSM.StateCanExit() later (E.g. In OnLogic()).</summary>
        public virtual void OnExitRequest()
        {

        }


        /// <summary> Returns a string representation of all active states in the hierarchy (E.g. "/Move/Jump/Falling").</summary>
        public virtual string GetActiveHierarchyPath()
        {
            return Name.ToString();
        }
    }
}