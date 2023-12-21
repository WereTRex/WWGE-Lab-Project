using UnityEngine;
using UnityHFSM;

namespace WwGEProject.AI.Turret
{
    public class TurretShootingState : State
    {
        public override string Name { get => "Shooting"; }


        private readonly TurretController _brain;
        private readonly Gun _gun;

        private readonly float _maxRotationSpeed;
        private readonly float _rotationAcceleration;
        private readonly float _rotationDeceleration;

        private Vector3 _targetPosition;


        public TurretShootingState(TurretController brain, Gun gun, float maxSpeed, float acceleration, float deceleration)
        {
            this._brain = brain;
            this._gun = gun;

            this._maxRotationSpeed = maxSpeed;
            this._rotationAcceleration = acceleration;
            this._rotationDeceleration = deceleration;
        }


        public override void OnEnter()
        {
            base.OnEnter();
            _gun.StartAttacking();
        }
        public override void OnLogic()
        {
            base.OnLogic();

            // If the brain still has a target, update the target position.
            if (_brain.Target != null)
                _targetPosition = _brain.Target.position;

            // Rotate to face the target.
            _brain.RotateToTarget(_targetPosition, _maxRotationSpeed, _rotationAcceleration, _rotationDeceleration);
        }
        public override void OnExit()
        {
            base.OnExit();
            _gun.StopAttacking();
        }
    }
}