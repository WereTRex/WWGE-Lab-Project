using System;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class Inside : SubStateMachine<Action>
    {
        public override string Name { get => "Inside"; }
    }
}