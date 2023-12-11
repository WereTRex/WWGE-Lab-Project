using System;
using System.Collections;
using UnityEngine;

/// <summary> A projectile bullet.</summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Rigidbody _rb;
    [field: SerializeField] public Vector3 SpawnLocation { get; private set; }
    [SerializeField] private float _bulletLifetime = 2f;

    public Action<Bullet, Collision> OnCollision;


    private void Awake() => _rb = GetComponent<Rigidbody>();
    

    public void Spawn(Vector3 spawnForce)
    {
        // Cache our spawn location.
        SpawnLocation = transform.position;
        
        // Apply the spawn force.
        transform.forward = spawnForce.normalized;
        _rb.AddForce(spawnForce);

        // Force the disabling of this Bullet after a certain duration.
        StartCoroutine(DelayedDisable(_bulletLifetime));
    }

    private IEnumerator DelayedDisable(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnCollisionEnter(null);
    }

    private void OnCollisionEnter(Collision collision) => OnCollision?.Invoke(this, collision); // On collision, notify all methods subscribed to the OnCollision event.
    


    private void OnDisable()
    {
        // Stop any active coroutines.
        StopAllCoroutines();

        // Reset the velocity.
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Remove subscribers to the OnCollision event.
        OnCollision = null;
    }
}
