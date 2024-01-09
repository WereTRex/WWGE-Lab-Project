using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A component used in conjunction with Collider sto represent a Hurtbox.</summary>
public class Hurtbox : MonoBehaviour
{
    // Events.
    public Action<Collider> OnColliderEntered;
    public Action<Collider> OnColliderExited;

    // This object's colliders.
    private Collider[] _colliders;

    // Ensure this is a trigger.
    private void Awake()
    {
        _colliders = this.GetComponents<Collider>();

        foreach (Collider collider in _colliders)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        OnColliderEntered?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;

        OnColliderExited?.Invoke(other);
    }
}
