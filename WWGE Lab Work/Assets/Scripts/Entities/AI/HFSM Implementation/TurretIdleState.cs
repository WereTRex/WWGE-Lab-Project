using System.Collections;
using UnityEngine;
using UnityHFSM;

namespace WwGEProject.AI.Turret
{
    public class TurretIdleState : State
    {
        public override string Name { get => "Idle"; }


        private readonly TurretController _brain;
        private Coroutine _detectionCoroutine;


        private readonly Transform _rotationTarget;
        private readonly Quaternion[] _targetRotations;
        private int _rotationIndex;

        private readonly float _maxRotationSpeed;
        private readonly float _rotationAcceleration;
        private readonly float _rotationDeceleration;

        private readonly float _rotationPause;
        private float _rotationPauseTime;
        

        private const float FOUND_TARGET_BUFFER = 2f;
        private const float DETECTION_CHECK_DELAY = 0.25f;


        /// <summary> Create a new instance of the TurretIdleState class.</summary>
        /// <param name="rotationTarget"> The target of the rotation applied by this state,</param>
        /// <param name="targetRotations"> An array of rotations to look between.</param>
        /// <param name="maxRotationSpeed"> The maximum rotation speed of the turret.</param>
        /// <param name="rotationAcceleration"> The acceleration rate of the turret.</param>
        /// <param name="rotationDeceleration"> The deceleration rate of the turret.</param>
        /// <param name="rotationPause"> The time that this turret spends not moving after arriving at a target rotation.</param>
        /// <inheritdoc cref="State(bool, bool)"/>
        public TurretIdleState(TurretController brain, Transform rotationTarget, Quaternion[] targetRotations, float maxRotationSpeed, float rotationAcceleration, float rotationDeceleration, float rotationPause, bool needsExitTime = false, bool isGhostState = false) : base(needsExitTime, isGhostState)
        {
            this._brain = brain;

            this._rotationTarget = rotationTarget;
            this._targetRotations = targetRotations;

            this._maxRotationSpeed = maxRotationSpeed;
            this._rotationAcceleration = rotationAcceleration;
            this._rotationDeceleration = rotationDeceleration;
            this._rotationPause = rotationPause;
        }


        public override void OnEnter()
        {
            base.OnEnter();

            _rotationIndex = 0;

            if (_detectionCoroutine != null)
                _brain.StopCoroutine(_detectionCoroutine);
            _detectionCoroutine = _brain.StartCoroutine(TryDetection());
        }
        public override void OnExit()
        {
            base.OnExit();

            if (_detectionCoroutine != null)
                _brain.StopCoroutine(_detectionCoroutine);
        }

        public override void OnLogic()
        {
            base.OnLogic();

            // If we are currently in a rotation pause, then return.
            if (_rotationPauseTime > Time.time)
                return;


            // Rotate to face the target.
            _brain.RotateToTarget(_targetRotations[_rotationIndex], _maxRotationSpeed, _rotationAcceleration, _rotationDeceleration);
            

            // Increment the rotation index.
            if (Quaternion.Angle(_targetRotations[_rotationIndex], _rotationTarget.rotation) < FOUND_TARGET_BUFFER)
            {
                _rotationPauseTime = Time.time + _rotationPause;
                _brain.ResetRotationSpeeds();

                // Increment the rotation index (Looping to 0 to avoid exceeding it).
                if (_rotationIndex < _targetRotations.Length - 1)
                    _rotationIndex++;
                else
                    _rotationIndex = 0;
            }
        }

        private IEnumerator TryDetection()
        {
            while (true)
            {
                _brain.TryGetTarget();
                yield return new WaitForSeconds(DETECTION_CHECK_DELAY);
            }
        }
    }
}