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


            // Rotate towards the current target.
            if (_pauseTime > Time.time || _brain.Target == null)
                return;


            // Rotate to face the target.
            _brain.RotateToTarget(_brain.Target.position, _maxRotationSpeed, _rotationAcceleration, _rotationDeceleration);
        }
    }
}