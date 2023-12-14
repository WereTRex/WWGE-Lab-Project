using System;

namespace UnityHFSM
{
    /// <summary> A class used to determine whether the State Machine should transition to another state depending on a dynamically computed delay and an optional condition.</summary>
    public class TransitionAfterDynamic<TStateID> : TransitionBase<TStateID>
    {
        private ITimer _timer;
        private float _delay;

        private bool _onlyEvaluateDelayOnEnter;
        private Func<TransitionAfterDynamic<TStateID>, float> _delayCalculator;

        private Func<TransitionAfterDynamic<TStateID>, bool> _condition;

        private Action<TransitionAfterDynamic<TStateID>> _beforeTransition;
        private Action<TransitionAfterDynamic<TStateID>> _afterTransition;


        /// <summary> Initialises a new instance of the TransitionAfterDynamic class.</summary>
        /// <param name="delay"> A function that dynamically computes the delay time.</param>
        /// <param name="condition"> A function that returns true if the State Machine should transition to the <c>To</c> state</param>
        /// <param name="onlyEvaluateDelayOnEnter"> If true, the dunamic delay is only recalculated when the <c>From</c> state is entered.
        ///     If false, the delay is evaluated with each logic step.</param>
        /// <inheritdoc cref="Transition{TStateID}(
        ///     TStateID,
        ///     TStateID,
        ///     Func{Transition{TStateID}, bool}
        ///     Action{Transition{TStateID}},
        ///     Action{Transition{TStateID}},
        ///     bool)"/>
        public TransitionAfterDynamic(
            TStateID from,
            TStateID to,
            Func<TransitionAfterDynamic<TStateID>, float> delay,
            Func<TransitionAfterDynamic<TStateID>, bool> condition = null,
            bool onlyEvaluateDelayOnEnter = false,
            Action<TransitionAfterDynamic<TStateID>> onTransition = null,
            Action<TransitionAfterDynamic<TStateID>> afterTransition = null,
            bool forceInstantly = false) : base(from, to, forceInstantly)
        {
            this._delayCalculator = delay;
            this._condition = condition;
            this._onlyEvaluateDelayOnEnter = onlyEvaluateDelayOnEnter;
            this._beforeTransition = onTransition;
            this._afterTransition = afterTransition;

            this._timer = new Timer();
        }


        public override void OnEnter()
        {
            _timer.Reset();

            if (_onlyEvaluateDelayOnEnter)
                _delay = _delayCalculator(this);
        }

        public override bool ShouldTransition()
        {
            // Get the current evaluation of the delay from the delay calculator if we don't only calculate it on Enter.
            if (!_onlyEvaluateDelayOnEnter)
                _delay = _delayCalculator(this);

            // If the elapsed time is not yet greater than the required delay, don't transition.
            if (_timer.Elapsed < _delay)
                return false;

            // If we don't have a condition, then we should transition.
            if (_condition == null)
                return false;

            // Otherwise, test the condition.
            return _condition(this);
        }


        public override void BeforeTransition() => _beforeTransition?.Invoke(this);
        public override void AfterTransition() => _afterTransition?.Invoke(this);
    }


    #region Overloaded Implementations
    // Overloaded Classes allow for an easier useage of the class for common cases.

    /// <inheritdoc />
    public class TransitionAfterDynamic : TransitionAfterDynamic<string>
    {
        /// <inheritdoc />
        public TransitionAfterDynamic(
            string from,
            string to,
            Func<TransitionAfterDynamic<string>, float> delay,
            Func<TransitionAfterDynamic<string>, bool> condition = null,
            bool onlyEvaluateDelayOnEnter = false,
            Action<TransitionAfterDynamic<string>> onTransition = null,
            Action<TransitionAfterDynamic<string>> afterTransition = null,
            bool forceInstantly = false)
            : base(from, to, delay, condition, onlyEvaluateDelayOnEnter: onlyEvaluateDelayOnEnter, onTransition: onTransition, afterTransition: afterTransition, forceInstantly: forceInstantly)
        {

        }
    }
    #endregion
}