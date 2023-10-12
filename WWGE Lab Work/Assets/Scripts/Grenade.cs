using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private float _explosionForce = 500f;
    [SerializeField] private float _fuseTime = 2f;


    private void Start()
    {
        StartCoroutine(Fuse());
    }
    
    IEnumerator Fuse()
    {
        yield return new WaitForSeconds(_fuseTime);
        Explode();
    }


    void Explode()
    {
        foreach (Collider hitObject in Physics.OverlapSphere(transform.position, _explosionRadius))
        {
            if (hitObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, 0.1f);
            }
        }
    }
}
