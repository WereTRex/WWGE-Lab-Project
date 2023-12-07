using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A base state to be inherited by other turret states that must rotate to face a target.</summary>
public abstract class TurretRotationState : IState
{
    protected readonly EnemyTurret Brain;
    protected readonly Transform RotationTarget;
    protected readonly float RotationSpeed;

    public bool FacingTargetDirection = false;
    protected Vector3 TargetDir;

    public TurretRotationState(EnemyTurret brain, Transform rotTarget, float rotSpeed)
    {
        // Set readonly variables.
        this.Brain = brain;

        this.RotationTarget = rotTarget;
        this.RotationSpeed = rotSpeed;
    }


    public virtual void Tick()
    {
        // Get the direction to the target.
        if (Brain.Target != null)
            TargetDir = (Brain.Target.transform.position - RotationTarget.position).normalized;

        // Rotate towards the target direction.
        Vector3 newDirection = Vector3.RotateTowards(RotationTarget.forward, TargetDir, RotationSpeed * Time.deltaTime, 0.0f);
        RotationTarget.rotation = Quaternion.LookRotation(newDirection);

        // Check if we are facing the target.
        FacingTargetDirection = Vector3.Angle(TargetDir, RotationTarget.forward) < 5f;
    }

    public virtual void OnEnter()
    {
        Brain.StartDetection();
        TargetDir = RotationTarget.transform.forward;
    }
    public virtual void OnExit() => Brain.StopDetection();
}
