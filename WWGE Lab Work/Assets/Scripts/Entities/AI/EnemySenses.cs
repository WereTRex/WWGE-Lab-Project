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
        float greatestWeight = 0;
        withinSecondaryRadius = false;


        // Get all colliders within the maxDetectionRadius (That are in a targetLayer).
        foreach (Collider potentialTarget in Physics.OverlapSphere(transform.position, _maxDetectionRange, _targetLayers, QueryTriggerInteraction.Ignore))
        {
            // Is the target within the sight radius (Dot Product)? If so, discount it.
            float currentTargetDot = Vector3.Dot(transform.forward, (potentialTarget.transform.position - transform.position).normalized);
            if (!(currentTargetDot > _viewAngle))
                continue;

            // Is the collider obstructed? If so, discount it.
            if (Physics.Linecast(transform.position, potentialTarget.transform.position, _obstructionMask, QueryTriggerInteraction.Ignore))
                continue;

            // Is this collider a part of the same faction? If so, discount it.
            if (potentialTarget.TryGetComponent<EntityFaction>(out EntityFaction entityFaction))
                if (_factionScript.IsOpposingFaction(entityFaction.Faction) == false)
                    continue;


            // This collider is a valid collider (Within view angle and not obstructed).

            // Does this collider have a better weighted value than the current target?
            float percentageDistance = 1f - Vector3.Distance(transform.position, potentialTarget.transform.position) / _maxDetectionRange;
            float percentageDot = 1 - (currentTargetDot / _viewAngle);
            float currentWeight = (percentageDistance * 3f) + percentageDot;
            if (currentWeight > greatestWeight)
            {
                // This collider is the most valid collider (Closest to camera forwards) (ToDo: Change to a weighted comparison between distance and dot)
                greatestWeight = currentWeight;
                target = potentialTarget.transform;

                // Set for if we were within the secondary view angle (Used for some things like Turret's Shooting).
                if (currentTargetDot > _secondaryViewAngle)
                    withinSecondaryRadius = true;
                else
                    withinSecondaryRadius = false;
            }
        }


        // Return the found target (Which will be null if none were found).
        return target;
    }


    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos)
            return;

        // Gizmo Showing the Max Detection Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _maxDetectionRange);

        // Gizmo Showing the current facing direction.
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
}
