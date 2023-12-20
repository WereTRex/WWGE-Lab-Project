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

    [SerializeField] private float _viewAngle = 135f;
    [SerializeField] private float _secondaryViewAngle = 90f;

    [Space(5)]

    [SerializeField] private bool _drawGizmos;


    // Try to get a target within range, returning the most favourable target if there are none.
    public Transform TryGetTarget(out bool withinSecondaryRadius, float distanceMultiplier = 1f, float angleMultiplier = 1f) => GetTarget(_maxDetectionRange * distanceMultiplier, _viewAngle * angleMultiplier, _secondaryViewAngle * angleMultiplier, out withinSecondaryRadius);


    // Attempts to return a valid target from the given distance, angle, & secondary and parameters.
    private Transform GetTarget(float viewDistance, float viewAngle, float secondaryAngle, out bool withinSecondaryRadius)
    {
        Transform target = null;
        float greatestWeight = 0;
        withinSecondaryRadius = false;


        // Get all colliders within the maxDetectionRadius (That are in a targetLayer).
        foreach (Collider potentialTarget in Physics.OverlapSphere(transform.position, viewDistance, _targetLayers, QueryTriggerInteraction.Ignore))
        {
            // Get the angle from the origin of the senses to this potential target.
            float currentTargetAngle = Vector3.Angle(transform.forward, (potentialTarget.transform.position - transform.position).normalized);
            Debug.Log(currentTargetAngle);

            // If the target is outwith our vision angle, discount it.
            if (currentTargetAngle > (viewAngle / 2f))
                continue;

            // If the collider is obstructed, discount it (Don't target through obstructions).
            if (Physics.Linecast(transform.position, potentialTarget.transform.position, _obstructionMask, QueryTriggerInteraction.Ignore))
                continue;

            // If this collider is a part of the same faction, discount it (Don't target allies).
            if (potentialTarget.TryGetComponent<EntityFaction>(out EntityFaction entityFaction))
                if (_factionScript.IsAllyFaction(entityFaction.Faction))
                    continue;


            // Does this collider have a better weighted value than the current target?
            float currentWeight = GetWeightedInterest(
                distance: Vector3.Distance(transform.position, potentialTarget.transform.position),
                maxDistance: viewDistance,
                angle: currentTargetAngle / 2f,
                maxAngle: viewAngle);

            if (currentWeight > greatestWeight)
            {
                // This collider is the most valid collider (Closest to camera forwards) (ToDo: Change to a weighted comparison between distance and dot)
                greatestWeight = currentWeight;
                target = potentialTarget.transform;

                // Set for if we were within the secondary view angle (Used for some things like Turret's Shooting).
                withinSecondaryRadius = currentTargetAngle <= (secondaryAngle / 2f);
            }
        }

        // Return the found target (Which will be null if none were found).
        return target;
    }


    private float GetWeightedInterest(float distance, float maxDistance, float angle, float maxAngle)
    {
        // Calculate the percentage of how close the target is (Both for distance and angle).
        float percentageDistance = 1f - (distance / maxDistance);
        float percentageAngle = 1f - (angle / maxAngle);

        // Return a value weighted towards a closer distance than angle.
        return (percentageDistance * 3f) + percentageAngle;
    }



    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos)
            return;

        // Gizmo Showing the Max Detection Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _maxDetectionRange);

        // (Gizmo) Show the max view angles.
        {
            Gizmos.color = Color.red;
            Vector3 leftDirection = Quaternion.AngleAxis(-(_viewAngle / 2f), transform.up) * transform.forward;
            Vector3 rightDirection = Quaternion.AngleAxis(_viewAngle / 2f, transform.up) * transform.forward;

            Gizmos.DrawRay(transform.position, leftDirection * _maxDetectionRange);
            Gizmos.DrawRay(transform.position, rightDirection * _maxDetectionRange);
        }

        // (Gizmo) Show the secondary view angles.
        {
            Gizmos.color = Color.blue;
            Vector3 leftDirection = Quaternion.AngleAxis(-(_secondaryViewAngle / 2f), transform.up) * transform.forward;
            Vector3 rightDirection = Quaternion.AngleAxis(_secondaryViewAngle / 2f, transform.up) * transform.forward;

            Gizmos.DrawRay(transform.position, leftDirection * _maxDetectionRange);
            Gizmos.DrawRay(transform.position, rightDirection * _maxDetectionRange);
        }


        // Gizmo Showing the current facing direction.
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
    }
}
