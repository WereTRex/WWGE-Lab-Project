namespace UnityHFSM
{
    // Note: This is useful as it allows the parent FSM to be independent from the sub-states, reducing complexity.
    /// <summary> A subset of features that every parent state machine must provide.</summary>
    public interface IStateMachine
    {
        /// <summary> Tells the State Machine to perform any pending state transition requests.</summary>
        void StateCanExit();

        bool HasPendingTransition { get; }
        IStateMachine ParentFSM { get; }
    }
}