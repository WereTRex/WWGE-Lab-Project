using System;
using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class Outside : SubStateMachine<Action>
    {
        public override string Name { get => "Outside"; }
    }
}