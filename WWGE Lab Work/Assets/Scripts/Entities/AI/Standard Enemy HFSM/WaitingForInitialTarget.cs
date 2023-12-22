using System;
using UnityHFSM;


namespace EnemyStates.Standard
{
    public class WaitingForInitialTarget : State
    {
        public override string Name { get => "Waiting for Target"; }


        private readonly Func<bool> _hasTarget;

        public WaitingForInitialTarget(Func<bool> hasTarget) : base(needsExitTime: true, isGhostState: true)
        {
            this._hasTarget = hasTarget;
        }


        protected override bool CanExit() => _hasTarget();
    }
}