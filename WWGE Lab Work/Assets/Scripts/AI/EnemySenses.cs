using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySenses : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _sightMask;
    [SerializeField] private float _maxDetectionRange;

    [Space(5)]

    [SerializeField] private float _viewAngle = 0.45f;
    [SerializeField] private float _secondaryViewAngle = 0.6f;

    [Space(5)]

    [SerializeField] private bool _drawGizmos;


    // Try to get a target within range, returning the most favourable target if there are none.
    public Transform TryGetTarget(out bool withinSecondaryRadius)
    {
        Transform target = null;
        float closestDot = _viewAngle;

        // Get all colliders within the maxDetectionRadius (That are in a targetLayer).
        foreach (Collider potentialTarget in Physics.OverlapSphere(transform.position, _maxDetectionRange, _targetLayers))
        {            
            // Is the target within the sight radius (Dot Product)?
            float currentTargetDot = Vector3.Dot(transform.forward, (potentialTarget.transform.position - transform.position).normalized);
            if (!(currentTargetDot > _viewAngle))
                continue;

            // Is the collider obstructed?
            if (Physics.Linecast(transform.position, potentialTarget.transform.position, _sightMask))
                continue;

            // This collider is a valid collider (Within view angle and not obstructed).
            // Is this collider the closest to the turret's forward?
            if (currentTargetDot > closestDot)
            {
                // This collider is the most valid collider (Closest to camera forwards) (Change to a weighted comparison between distance and dot)
                closestDot = currentTargetDot;
                target = potentialTarget.transform;
            }
        }

        // Set for if we were within the secondary view angle (Used for some things like Turret's Shooting).
        withinSecondaryRadius = (closestDot > _secondaryViewAngle);

        // Return the found target (Which will be null if none were found).
        return target;
    }


    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _maxDetectionRange);
    }
}
