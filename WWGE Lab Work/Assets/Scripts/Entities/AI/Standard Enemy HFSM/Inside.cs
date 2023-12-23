using System;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class Inside : SubStateMachine<Action>
    {
        public override string Name { get => "Inside"; }


        private readonly StandardEnemy _brain;

        public Inside(StandardEnemy brain)
        {
            this._brain = brain;
        }


        public override void OnEnter()
        {
            base.OnEnter();

            _brain.StartDetection();
        }
        public override void OnExit()
        {
            base.OnExit();

            _brain.StopDetection();
        }
    }
}