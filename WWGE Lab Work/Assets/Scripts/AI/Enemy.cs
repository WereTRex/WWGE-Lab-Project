using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected Transform _target;
    public Transform GetTarget() => _target;
    public void SetTarget(Transform newTarget) => _target = newTarget;


    public Transform InitialTarget { get; protected set; }
    public HealthComponent InitialTargetHealth {
        get {
            if (cachedTargetHealth == null && InitialTarget != null)
            {
                InitialTarget.TryGetComponent<HealthComponent>(out cachedTargetHealth);
            }

            return cachedTargetHealth;
        }
    }
    private HealthComponent cachedTargetHealth;

    [SerializeField] protected HealthComponent _healthComponent;


    public void SetInitialTarget(Transform target)
    {
        this.InitialTarget = target;
    }
}
