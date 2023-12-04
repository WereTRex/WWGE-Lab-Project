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


    public void SetupHurtbox(Vector3 scale) => SetupHurtbox(scale.x, scale.y, scale.z);
    public void SetupHurtbox(float x, float y = 0f, float z = 0f)
    {
        y = y == 0 ? x : y;
        z = z == 0 ? x : z;

        
        if (_collider.GetType() == typeof(BoxCollider))
        {
            (_collider as BoxCollider).size = new Vector3(x, y, z);
            (_collider as BoxCollider).center = new Vector3(x / 2, 0, 0);
        }
        else if (_collider.GetType() == typeof(CapsuleCollider))
        {
            (_collider as CapsuleCollider).radius = x;
            (_collider as CapsuleCollider).height = y;
        } 
        else if (_collider.GetType() == typeof(SphereCollider))
        {
            (_collider as SphereCollider).radius = x;
        }
    }
}
