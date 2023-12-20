using UnityHFSM;

namespace WwGEProject.AI.Turret
{
    public class TurretDeactivatedState : State
    {
        public override string Name { get => "Deactivated"; }


        private readonly Repairable _repairableComponent;

        public TurretDeactivatedState(Repairable repairableComponent   )
        {
            this._repairableComponent = repairableComponent;
        }


        public override void Init()
        {
            base.Init();
            _repairableComponent.enabled = false;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            _repairableComponent.enabled = true;
        }
        public override void OnExit()
        {
            base.OnExit();
            _repairableComponent.enabled = false;
        }
    }
}