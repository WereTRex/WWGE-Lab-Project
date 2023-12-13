using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactReciever : MonoBehaviour
{
    [SerializeField] private float _mass = 3f;
    private Vector3 _impact = Vector3.zero;


    private CharacterController _controller;
    private void Start() => _controller = GetComponent<CharacterController>();


    private void Update()
    {
        if (_impact.magnitude > 0.2f)
            _controller.Move(_impact * Time.deltaTime);

        _impact = Vector3.Lerp(_impact, Vector3.zero, 5f * Time.deltaTime);
    }

    public void AddExplosionForce(float force, Vector3 explosionPosition, float explosionRadius, float upwardsModifier)
    {
        Vector3 direction = (transform.position - explosionPosition).normalized;
        float distance = Vector3.Distance(transform.position, explosionPosition);

        if (direction.y < 0)
            direction.y = -direction.y;

        _impact = direction.normalized * (force / _mass) * (1 - distance / explosionRadius);
    }
    public void AddImpactForce(Vector3 direction, float force)
    {
        direction.Normalize();

        // (Remove?) Reflect downwards forces off the ground.
        if (direction.y < 0)
            direction.y = -direction.y;

        _impact = direction.normalized * force / _mass;
    }
}
