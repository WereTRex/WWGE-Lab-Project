using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public Action<Collider> OnColliderEntered;
    public Action<Collider> OnColliderExited;

    private Collider _collider;

    // Ensure this is a trigger.
    private void Awake()
    {
        _collider = this.GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => OnColliderEntered?.Invoke(other);
    private void OnTriggerExit(Collider other) => OnColliderExited?.Invoke(other);


    public void SetupHurtbox(float depth, float width = 0f, float height = 0f)
    {
        width = width == 0 ? depth : width;
        height = height == 0 ? depth : height;

        
        if (_collider.GetType() == typeof(BoxCollider))
        {
            (_collider as BoxCollider).size = new Vector3(depth, height, width);
            (_collider as BoxCollider).center = new Vector3(depth / 2, 0, 0);
        }
        else if (_collider.GetType() == typeof(CapsuleCollider))
        {
            (_collider as CapsuleCollider).radius = depth;
            (_collider as CapsuleCollider).height = height;
        } 
        else if (_collider.GetType() == typeof(SphereCollider))
        {
            (_collider as SphereCollider).radius = depth;
        }
    }
}
