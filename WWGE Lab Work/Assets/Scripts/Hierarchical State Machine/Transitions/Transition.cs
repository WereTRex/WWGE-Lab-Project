using System;

namespace UnityHFSM
{
    /// <summary> A class used to determine whether the State Machine should transition to another state.</summary>
    public class Transition<TStateID> : TransitionBase<TStateID>
    {
        public Func<Transition<TStateID>, bool> Condition;
        private Action<Transition<TStateID>> _beforeTransition;
        private Action<Transition<TStateID>> _afterTransition;


        /// <summary> Initialises a new instance of the Transition class.</summary>
        /// <param name="condition"> A function that returns true if the State Machine should transition to the <c>To</c> state.</param>
        /// <param name="onTransition"> A callback function that is called just before the transition.</param>
        /// <param name="afterTransition"> A callback function that is called just after the transition.</param>
        /// <inheritdoc cref="TransitionBase{TStateID}(TStateID, TStateID, bool)"/>
        public Transition(
            TStateID from,
            TStateID to,
            Func<Transition<TStateID>, bool> condition = null,
            Action<Transition<TStateID>> onTransition = null,
            Action<Transition<TStateID>> afterTransition = null,
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

    #region Overloaded Implemenetations
    // Overloaded Classes allow for an easier useage of the class for common cases.

    /// <inheritdoc/>
    public class Transition : Transition<string>
    {
        /// <inheritdoc/>
        public Transition(
            string from,
            string to,
            Func<Transition<string>, bool> condition = null,
            Action<Transition<string>> onTransition = null,
            Action<Transition<string>> afterTransition = null,
            bool forceInstantly = false) : base(from, to, condition, onTransition: onTransition, afterTransition: afterTransition, forceInstantly: forceInstantly)
        {

        }
    }
    #endregion
}