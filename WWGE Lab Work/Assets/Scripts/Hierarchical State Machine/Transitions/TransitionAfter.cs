using System;

namespace UnityHFSM
{
    /// <summary> A class used to determine whether the State Machine should transition to another state, depending on a delay and an optional condition.</summary>
    public class TransitionAfter<TStateID> : TransitionBase<TStateID>
    {
        private float _delay;
        private ITimer _timer;

        private Func<TransitionAfter<TStateID>, bool> _condition;

        private Action<TransitionAfter<TStateID>> _beforeTransition;
        private Action<TransitionAfter<TStateID>> _afterTransition;



        public TransitionAfter(
            TStateID from,
            TStateID to,
            float delay,
            Func<TransitionAfter<TStateID>, bool> condition = null,
            Action<TransitionAfter<TStateID>> onTransition = null,
            Action<TransitionAfter<TStateID>> afterTransition = null,
            bool forceInstantly = false) : base(from, to, forceInstantly)
        {
            this._delay = delay;
            this._condition = condition;
            this._beforeTransition = onTransition;
            this._afterTransition = afterTransition;

            this._timer = new Timer();
        }


        public override void OnEnter() => _timer.Reset();

        public override bool ShouldTransition()
        {
            // If the timer has not yet elapsed, then we shouldn't transition.
            if (_timer.Elapsed < _delay)
                return false;

            // If we don't have a required condition, then we should transition.
            if (_condition == null)
                return true;

            // Otherwise, test the transition.
            return _condition(this);
        }

        public override void BeforeTransition() => _beforeTransition?.Invoke(this);
        public override void AfterTransition() => _afterTransition?.Invoke(this);
    }

    #region Overloaded Implementations
    // Overloaded Classes allow for an easier useage of the class for common cases.

    /// <inheritdoc/>
    public class TransitionAfter : TransitionAfter<string>
    {
        /// <inheritdoc/>
        public TransitionAfter(
            string from,
            string to,
            float delay,
            Func<TransitionAfter<string>, bool> condition = null,
            Action<TransitionAfter<string>> onTransition = null,
            Action<TransitionAfter<string>> afterTransition = null,
            bool forceInstantly = false) : base(from, to, delay, condition, onTransition: onTransition, afterTransition: afterTransition, forceInstantly: forceInstantly)
        {

        }
    }
    #endregion
}