using UnityEngine;
using UnityHFSM;

namespace WwGEProject.AI.Turret
{
    public class TurretAlertState : State
    {
        public override string Name { get => "Alert"; }


        private readonly TurretController _brain;

        private readonly float _maxRotationSpeed;
        private readonly float _rotationAcceleration;
        private readonly float _rotationDeceleration;

        private readonly float _initialPause;
        private float _pauseTime;

        private Vector3 _targetPosition;


        public TurretAlertState(TurretController brain, float maxSpeed, float acceleration, float deceleration, float initialPause)
        {
            this._brain = brain;

            this._maxRotationSpeed = maxSpeed;
            this._rotationAcceleration = acceleration;
            this._rotationDeceleration = deceleration;

            this._initialPause = initialPause;
        }


        public override void OnEnter()
        {
            base.OnEnter();
            _pauseTime = Time.time + _initialPause;
        }
        public override void OnLogic()
        {
            base.OnLogic();

            // Don't rotate if we are in the pause.
            if (_pauseTime > Time.time)
                return;

            // If the brain still has a target, update the target position.
            if (_brain.Target != null)
                _targetPosition = _brain.Target.position;

            // Rotate to face the target.
            _brain.RotateToTarget(_targetPosition, _maxRotationSpeed, _rotationAcceleration, _rotationDeceleration);
        }
    }
}