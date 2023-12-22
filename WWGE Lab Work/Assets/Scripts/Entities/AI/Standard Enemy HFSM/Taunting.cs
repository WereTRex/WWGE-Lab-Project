using UnityEngine;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class Taunting : State
    {
        public override string Name { get => "Taunting"; }


        private readonly float _tauntDuration;
        private float _tauntCompleteTime;


        public Taunting(float tauntDuration) : base(needsExitTime: true) => this._tauntDuration = tauntDuration;


        public override void OnEnter() => _tauntCompleteTime = Time.time + _tauntDuration;
        protected override bool CanExit() => _tauntCompleteTime <= Time.time;
    }
}