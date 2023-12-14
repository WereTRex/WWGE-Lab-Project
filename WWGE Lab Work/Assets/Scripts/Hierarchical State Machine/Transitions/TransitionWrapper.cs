using System;

namespace UnityHFSM
{
    /// <summary> A class that allows you to run additional functions (Companion Code) before and after the wrapped state's code.</summary>
    public class TransitionWrapper<TStateID>
    {
        public class WrappedTransition : TransitionBase<TStateID>
        {
           private Action<TransitionBase<TStateID>>
                _beforeOnEnter, _afterOnEnter,
                _beforeShouldTransition, _afterShouldTransition;

            private TransitionBase<TStateID> _transition;


            public WrappedTransition(
                TransitionBase<TStateID> transition,
                Action<TransitionBase<TStateID>> beforeOnEnter,
                Action<TransitionBase<TStateID>> afterOnEnter,
                Action<TransitionBase<TStateID>> beforeShouldTransition,
                Action<TransitionBase<TStateID>> afterShouldTransition
                ) : base(transition.From, transition.To, forceInstantly: transition.ForceInstantly)
            {
                this._transition = transition;

                this._beforeOnEnter = beforeOnEnter;
                this._afterOnEnter = afterOnEnter;

                this._beforeShouldTransition = beforeShouldTransition;
                this._afterShouldTransition = afterShouldTransition;
            }


            public override void Init() => _transition.FSM = this.FSM;

            public override void OnEnter()
            {
                _beforeOnEnter?.Invoke(this);
                _transition.OnEnter();
                _afterOnEnter?.Invoke(this);
            }

            public override bool ShouldTransition()
            {
                _beforeShouldTransition?.Invoke(this);
                bool shouldTransition = _transition.ShouldTransition();
                _afterShouldTransition?.Invoke(this);

                return shouldTransition;
            }

            public override void BeforeTransition() => _transition.BeforeTransition();
            public override void AfterTransition() => _transition.AfterTransition();
        }

        private Action<TransitionBase<TStateID>>
            _beforeOnEnter, _afterOnEnter,
            _beforeShouldTransition, _afterShouldTransition;


        public TransitionWrapper(
            Action<TransitionBase<TStateID>> beforeOnEnter = null,
            Action<TransitionBase<TStateID>> afterOnEnter = null,
            Action<TransitionBase<TStateID>> beforeShouldTransition = null,
            Action<TransitionBase<TStateID>> afterShouldTransition = null)
        {
            this._beforeOnEnter = beforeOnEnter;
            this._afterOnEnter = afterOnEnter;

            this._beforeShouldTransition = beforeShouldTransition;
            this._afterShouldTransition = afterShouldTransition;
        }


        public WrappedTransition Wrap(TransitionBase<TStateID> transition) => new WrappedTransition(transition, _beforeOnEnter, _afterOnEnter, _beforeShouldTransition, _afterShouldTransition);
    }

    #region Overloaded Implementations
    // Overloaded Classes allow for an easier useage of the class for common cases.

    /// <inheritdoc />
    public class TransitionWrapper : TransitionWrapper<string>
    {
        public TransitionWrapper(
            Action<TransitionBase<string>> beforeOnEnter = null,
            Action<TransitionBase<string>> afterOnEnter = null,

            Action<TransitionBase<string>> beforeShouldTransition = null,
            Action<TransitionBase<string>> afterShouldTransition = null
            ) : base(beforeOnEnter, afterOnEnter, beforeShouldTransition, afterShouldTransition)
        {

        }
    }
    #endregion
}