using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Rigidbody _rb;
    [field: SerializeField] public Vector3 SpawnLocation { get; private set; }
    [SerializeField] private float _bulletLifetime = 2f;

    public Action<Bullet, Collision> OnCollision;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Spawn(Vector3 spawnForce)
    {
        SpawnLocation = transform.position;
        transform.forward = spawnForce.normalized;
        _rb.AddForce(spawnForce);
        StartCoroutine(DelayedDisable(_bulletLifetime));
    }

    private IEnumerator DelayedDisable(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnCollisionEnter(null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision?.Invoke(this, collision);
    }


    private void OnDisable()
    {
        StopAllCoroutines();
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        OnCollision = null;
    }
}
