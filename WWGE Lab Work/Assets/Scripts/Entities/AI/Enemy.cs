using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : StateBasedEntity
{
    [SerializeField] protected Transform Target;
    public Transform GetTarget() => Target;
    public void SetTarget(Transform newTarget) => Target = newTarget;


    public Transform InitialTarget { get; protected set; }
    public HealthComponent InitialTargetHealth {
        get {
            // If we haven't cached an initialHealthComponent, then cache one.
            if (_cachedTargetHealth == null && InitialTarget != null)
            {
                InitialTarget.TryGetComponent<HealthComponent>(out _cachedTargetHealth);
            }

            // Return the Cached Health Component.
            return _cachedTargetHealth;
        }
    }
    private HealthComponent _cachedTargetHealth;

    [SerializeField] protected HealthComponent HealthComponent;


    public void SetInitialTarget(Transform target)
    {
        this.InitialTarget = target;
    }
}
