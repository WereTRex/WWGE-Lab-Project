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

    [Space(5)]

    [Tooltip("How long until this grenade detonates on its own. (<0 For Infinite)"), Min(-1)]
        [SerializeField] private float _fuseTime = 2f;
    [Tooltip("How many collisions this grenade can make before detonating (-1 For Infinite, 0 For Single Impact)"), Min(-1)]
        [SerializeField] private int _maxCollisions = 0;
    private int _collisionsRemaining;

    [Space(5)]

    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _explosionLifetime = 5f;


    private void Start()
    {
        _collisionsRemaining = _maxCollisions;
        StartCoroutine(Fuse());
    }
    
    // Detonate after a certain time.
    IEnumerator Fuse()
    {
        // There is no fuse if the fuseTime is under 0.
        if (_fuseTime < 0)
            yield break;

        yield return new WaitForSeconds(_fuseTime);
        Explode();
    }
    private void OnCollisionEnter(Collision collision)
    {
        // If this grenade shouldn't detonate from collisions, stop the check.
        if (_maxCollisions == -1)
            return;

        _collisionsRemaining--;
        if (_collisionsRemaining < 0)
            Explode();
    }


    void Explode()
    {
        // Create a Explosion Effect which destroys itself after '_explosionLifetime' seconds.
        Destroy(Instantiate(_explosionPrefab, transform.position, Quaternion.identity), _explosionLifetime);

        // Add an explosion force & deal damage to all objects in range.
        foreach (Collider hitObject in Physics.OverlapSphere(transform.position, _explosionRadius))
        {
            // Add explosion force.
            if (hitObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, 0.1f);
            else if (hitObject.TryGetComponent<ImpactReciever>(out ImpactReciever impactReciever))
                impactReciever.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, 0.1f);


            // Deal damage scaled by distance.
            if (hitObject.TryGetComponent<HealthComponent>(out HealthComponent healthComponent))
            {
                float distance = Vector3.Distance(transform.position, hitObject.ClosestPoint(transform.position));
                healthComponent.TakeDamage(_damageCurve.Evaluate(distance / _explosionRadius));
            }
        }

        // Destroy this Grenade.
        Destroy(gameObject);
    }
}
