using UnityEngine;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class Stagger : State
    {
        public override string Name { get => "Stagger"; }


        private readonly float _staggerTime;
        private float _staggerCompleteTime;


        public Stagger(float staggerTime) : base (needsExitTime: true) => this._staggerTime = staggerTime;


        public override void OnEnter() => _staggerCompleteTime = Time.time + _staggerTime;
        protected override bool CanExit() => _staggerCompleteTime <= Time.time;
    }
}