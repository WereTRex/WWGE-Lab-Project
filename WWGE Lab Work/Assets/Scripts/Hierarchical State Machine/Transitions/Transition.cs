using System;

namespace UnityHFSM
{
    /// <summary> A class used to determine whether the State Machine should transition to another state.</summary>
    public class Transition : TransitionBase
    {
        public Func<Transition, bool> Condition;
        private Action<Transition> _beforeTransition;
        private Action<Transition> _afterTransition;


        /// <summary> Initialises a new instance of the Transition class.</summary>
        /// <param name="condition"> A function that returns true if the State Machine should transition to the <c>To</c> state.</param>
        /// <param name="onTransition"> A callback function that is called just before the transition.</param>
        /// <param name="afterTransition"> A callback function that is called just after the transition.</param>
        /// <inheritdoc cref="TransitionBase(IState, IState, bool)"/>
        public Transition(
            IState from,
            IState to,
            Func<Transition, bool> condition = null,
            Action<Transition> onTransition = null,
            Action<Transition> afterTransition = null,
            bool forceInstantly = false) : base(from, to, forceInstantly)
        {
            this.Condition = condition;
            this._beforeTransition = onTransition;
            this._afterTransition = afterTransition;
        }


        public override bool ShouldTransition()
        {
            // If this transition has no condition, then we should transition.
            if (Condition == null)
                return true;

            // Otherwise return the returned value of the condition.
            return Condition(this);
        }


        public override void BeforeTransition() => _beforeTransition?.Invoke(this);
        public override void AfterTransition() => _afterTransition?.Invoke(this);
    }
}