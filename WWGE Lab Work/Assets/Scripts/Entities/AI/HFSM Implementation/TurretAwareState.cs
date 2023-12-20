using UnityEngine;
using UnityHFSM;

namespace WwGEProject.AI.Turret
{
    public class TurretAwareState<TEvent> : SubStateMachine<TEvent>
    {
        public override string Name { get => "Aware"; }
    }
}