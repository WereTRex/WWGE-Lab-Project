using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySenses : MonoBehaviour
{
    [SerializeField] private EntityFaction _factionScript;

    [Space(5)]
    
    [SerializeField] private LayerMask _targetLayers;
    [SerializeField] private LayerMask _obstructionMask;
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
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.blue, 0.25f);

        // Get all colliders within the maxDetectionRadius (That are in a targetLayer).
        foreach (Collider potentialTarget in Physics.OverlapSphere(transform.position, _maxDetectionRange, _targetLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Potential Target: " + potentialTarget);
            Debug.Log("Dot Product: " + Vector3.Dot(transform.forward, (potentialTarget.transform.position - transform.position).normalized));
            
            // Is the target within the sight radius (Dot Product)? If so, discount it.
            float currentTargetDot = Vector3.Dot(transform.forward, (potentialTarget.transform.position - transform.position).normalized);
            if (!(currentTargetDot > _viewAngle))
                continue;

            Debug.Log("Within View Angle");

            // Is the collider obstructed? If so, discount it
            if (Physics.Linecast(transform.position, potentialTarget.transform.position, _obstructionMask, QueryTriggerInteraction.Ignore))
                continue;

            Debug.Log("Unobstructed");

            // Is this collider a part of the same faction? If so, discount it
            if (potentialTarget.TryGetComponent<EntityFaction>(out EntityFaction entityFaction))
                if (_factionScript.IsOpposingFaction(entityFaction.Faction) == false)
                    continue;

            Debug.Log("Valid Target: " + potentialTarget);


            // This collider is a valid collider (Within view angle and not obstructed).
            // Is this collider the closest to the turret's forward?
            if (currentTargetDot > closestDot)
            {
                // This collider is the most valid collider (Closest to camera forwards) (Change to a weighted comparison between distance and dot)
                closestDot = currentTargetDot;
                target = potentialTarget.transform;
            }
        }
        Debug.Log(target);

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
