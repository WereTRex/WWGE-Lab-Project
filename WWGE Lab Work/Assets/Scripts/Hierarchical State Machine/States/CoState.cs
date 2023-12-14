using System;
using System.Collections;
using UnityEngine;

namespace UnityHFSM
{
    /// <summary> A state that can run a UnityCoroutine as its OnLogic method.</summary>
    public class CoState<TStateID, TEvent> : ActionState<TStateID, TEvent>
    {
        private MonoBehaviour _mono;

        private Func<IEnumerator> _coroutineCreator;
        private Action<CoState<TStateID, TEvent>> _onEnter;
        private Action<CoState<TStateID, TEvent>> _onExit;
        private Func<CoState<TStateID, TEvent>, bool> _canExit;

        private bool _shouldLoopCoroutine;


        public ITimer Timer;

        private Coroutine _activeCoroutine;


        #region Constructors
        // The CoState class allows you to use either a function without any parameters
        //  or a function that takes the state as a parameter to create the coroutine.
        //  To allow for this and ease of use, the class has two nearly identical constructors.

        /// <summary> Initialises a new instance of the CoState class.</summary>
        /// <param name="mono"> The MonoBehaviour of the script that should run the coroutine.</param>
        /// <param name="coroutine"> A coroutine that is run while this state is active.
        ///     It runs independently from the paren State Machine's OnLogic(), because it is handled by Unity.
        ///     It is started once the state enters and is terminated when the state exits.</param>
        /// <param name="onEnter"> A function that is called when the State Machine enters this state.</param>
        /// <param name="onExit"> A function that is called when teh State Machine exits this state.</param>
        /// <param name="canExit"> (Only if needsExitTime is true):
        ///     A function that determines if the state is ready to exit (True) or not (False).
        ///     It is called in OnExitRequest and on each logic step when a transition is pending.</param>
        /// <param name="loop"> If true, it will loop the coroutine, running it again once it has completed.</param>
        /// <inheritdoc cref="StateBase{T}(bool, bool)"/>
        public CoState(
            MonoBehaviour mono,
            Func<CoState<TStateID, TEvent>, IEnumerator> coroutine,
            Action<CoState<TStateID, TEvent>> onEnter = null,
            Action<CoState<TStateID, TEvent>> onExit = null,
            Func<CoState<TStateID, TEvent>, bool> canExit = null,
            bool loop = true, bool needsExitTime = false, bool isGhostState = false) : base (needsExitTime, isGhostState)
        {
            this._mono = mono;
            this._coroutineCreator = () => coroutine(this);
            this._onEnter = onEnter;
            this._onExit = onExit;
            this._canExit = canExit;
            this._shouldLoopCoroutine = loop;

            Timer = new Timer();
        }


        /// <inheritdoc cref="CoState{TStateID, TEvent}(
		/// 	MonoBehaviour,
		/// 	Func{CoState{TStateID, TEvent}, IEnumerator},
		/// 	Action{CoState{TStateID, TEvent}},
		/// 	Action{CoState{TStateID, TEvent}},
		/// 	Func{CoState{TStateID, TEvent}, bool},
		/// 	bool,
		/// 	bool,
		/// 	bool
		/// )"/>
        public CoState(
            MonoBehaviour mono,
            Func<IEnumerator> coroutine,
            Action<CoState<TStateID, TEvent>> onEnter = null,
            Action<CoState<TStateID, TEvent>> onExit = null,
            Func<CoState<TStateID, TEvent>, bool> canExit = null,
            bool loop = true, bool needsExitTime = false, bool isGhostState = false) : base (needsExitTime, isGhostState)
        {
            this._mono = mono;
            this._coroutineCreator = coroutine;
            this._onEnter = onEnter;
            this._onExit = onExit;
            this._canExit = canExit;
            this._shouldLoopCoroutine = loop;

            Timer = new Timer();
        }
        #endregion


        public override void OnEnter()
        {
            Timer.Reset();

            _onEnter?.Invoke(this);

            if (_coroutineCreator != null)
                _activeCoroutine = _mono.StartCoroutine(_shouldLoopCoroutine ? LoopCoroutine() : _coroutineCreator());
        }

        private IEnumerator LoopCoroutine()
        {
            IEnumerator routine = _coroutineCreator();

            while (true)
            {
                // This checks if the coroutine needs at least one frame to execute.
                // If not, LoopCoroutine will wait 1 frame to avoid an infinite loop which will crash Unity.
                if (routine.MoveNext())
                    yield return routine.Current;
                else
                    yield return null;

                // Iterate from the onLogic coroutine until it is depleted.
                while(routine.MoveNext())
                    yield return routine.Current;


                // Restart the coroutine.
                routine = _coroutineCreator();
            }
        }


        public override void OnLogic()
        {
            // Check for whether we can transition.
            if (NeedsExitTime && _canExit != null & FSM.HasPendingTransition && _canExit(this))
                FSM.StateCanExit();
        }


        public override void OnExit()
        {
            // Stop the coroutine if it is still active
            if (_activeCoroutine != null)
            {
                _mono.StopCoroutine(_activeCoroutine);
                _activeCoroutine = null;
            }

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
    public class CoState<TStateID> : CoState<TStateID, string>
    {
        /// <inheritdoc/>
        public CoState(
            MonoBehaviour mono,
            Func<CoState<TStateID, string>, IEnumerator> coroutine,
            Action<CoState<TStateID, string>> onEnter = null,
            Action<CoState<TStateID, string>> onExit = null,
            Func<CoState<TStateID, string>, bool> canExit = null,
            bool loop = true, bool needsExitTime = false, bool isGhostState = false
            ) : base( mono, coroutine: coroutine, onEnter: onEnter, onExit: onExit, canExit: canExit, loop: loop, needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }

        /// <inheritdoc/>
        public CoState(
            MonoBehaviour mono,
            Func<IEnumerator> coroutine,
            Action<CoState<TStateID, string>> onEnter = null,
            Action<CoState<TStateID, string>> onExit = null,
            Func<CoState<TStateID, string>, bool> canExit = null,
            bool loop = true, bool needsExitTime = false, bool isGhostState = false
            ) : base(mono, coroutine: coroutine, onEnter: onEnter, onExit: onExit, canExit: canExit, loop: loop, needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }


    /// <inheritdoc/>
    public class CoState : CoState<string, string>
    {
        /// <inheritdoc/>
        public CoState(
            MonoBehaviour mono,
            Func<CoState<string, string>, IEnumerator> coroutine,
            Action<CoState<string, string>> onEnter = null,
            Action<CoState<string, string>> onExit = null,
            Func<CoState<string, string>, bool> canExit = null,
            bool loop = true, bool needsExitTime = false, bool isGhostState = false
            ) : base(mono, coroutine: coroutine, onEnter: onEnter, onExit: onExit, canExit: canExit, loop: loop, needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }

        /// <inheritdoc/>
        public CoState(
            MonoBehaviour mono,
            Func<IEnumerator> coroutine,
            Action<CoState<string, string>> onEnter = null,
            Action<CoState<string, string>> onExit = null,
            Func<CoState<string, string>, bool> canExit = null,
            bool loop = true, bool needsExitTime = false, bool isGhostState = false
            ) : base(mono, coroutine: coroutine, onEnter: onEnter, onExit: onExit, canExit: canExit, loop: loop, needsExitTime: needsExitTime, isGhostState: isGhostState)
        {

        }
    }
    #endregion
}