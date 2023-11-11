using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Tooltip("A damage curve representing the grenade's damage from directly beside the grenade (t = 0s) to the max range (t = 1s)")]
        [SerializeField] private AnimationCurve _damageCurve;
    
    [Space(5)]
    
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private float _explosionForce = 500f;
    [SerializeField] private float _fuseTime = 2f;

    [Space(5)]

    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _explosionLifetime = 5f;


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
        Destroy(Instantiate(_explosionPrefab, transform.position, Quaternion.identity), _explosionLifetime);

        foreach (Collider hitObject in Physics.OverlapSphere(transform.position, _explosionRadius))
        {
            if (hitObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, 0.1f);

            if (hitObject.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
            {
                float distance = Vector3.Distance(transform.position, hitObject.ClosestPoint(transform.position));
                healthComponent.TakeDamage(_damageCurve.Evaluate(distance / _explosionRadius));
            }
        }

        Destroy(gameObject);
    }
}
