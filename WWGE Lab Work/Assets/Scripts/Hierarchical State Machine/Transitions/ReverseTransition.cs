namespace UnityHFSM
{
    /// <summary> A ReverseTransition wraps another transition, but reverses it.
    ///     The "From" and "To" states are swapped. Only when the condition of the wrapped transition is false does it transition.
    ///     The BeforeTransition and AfterTransition callbacks of the wrapped function are also swapped.</summary>
    public class ReverseTransition<TStateID> : TransitionBase<TStateID>
    {
        private TransitionBase<TStateID> _wrappedTransition;
        private bool _shouldInitialiseWrappedTransition;

        public ReverseTransition(
            TransitionBase<TStateID> wrappedTransition,
            bool shouldInitialiseWrappedTransition = true
            ) : base(
                from:wrappedTransition.To,
                to: wrappedTransition.From,
                forceInstantly: wrappedTransition.ForceInstantly)
        {
            this._wrappedTransition = wrappedTransition;
            this._shouldInitialiseWrappedTransition = shouldInitialiseWrappedTransition;
        }

        public override void Init()
        {
            if (_shouldInitialiseWrappedTransition)
            {
                _wrappedTransition.FSM = this.FSM;
                _wrappedTransition.Init();
            }
        }

        public override void OnEnter() => _wrappedTransition.OnEnter();
        public override bool ShouldTransition() => !_wrappedTransition.ShouldTransition();
        public override void BeforeTransition() => _wrappedTransition.AfterTransition();
        public override void AfterTransition() => _wrappedTransition.BeforeTransition();
    }


    #region Overloaded Implementations
    // Overloaded Classes allow for an easier useage of the class for common cases.

    /// <inheritdoc/>
    public class ReverseTransition : ReverseTransition<string>
    {
        /// <inheritdoc/>
        public ReverseTransition(
            TransitionBase<string> wrappedTransition,
            bool shouldInitialiseWrappedTransition
            ) : base(wrappedTransition, shouldInitialiseWrappedTransition)
        {

        }
    }
    #endregion
}