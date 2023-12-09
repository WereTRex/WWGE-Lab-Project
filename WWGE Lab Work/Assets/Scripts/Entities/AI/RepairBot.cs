using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> A small bot that repairs damaged IRepairables within its detection range.</summary>
[RequireComponent(typeof(NavMeshAgent))]
public class RepairBot : MonoBehaviour
{
    private Repairable _target;
    
    private NavMeshAgent _agent;
    private Coroutine _detectObjectsCoroutine;
    private Coroutine _aiUpdateCoroutine;

    [SerializeField] private Vector3 _defaultPosition = Vector3.zero;

    [Space(5)]

    [SerializeField] private float _detectionUpdateDelay = 0.2f;
    [SerializeField] private int _maxDetections = 20;

    [SerializeField] private LayerMask _repairableLayers;
    [SerializeField] private float _detectionRadius = 7f;

    [Space(5)]

    [SerializeField] private float _aiUpdateDelay = 0.1f;
    [SerializeField] private float _repairRadius = 2f;


    private void Awake() => _agent = GetComponent<NavMeshAgent>();
    
    private void OnEnable()
    {
        // Activate our detection and aiUpdate coroutines.
        if (_detectObjectsCoroutine == null)
            _detectObjectsCoroutine = StartCoroutine(DetectRepairableObjects());
        if (_aiUpdateCoroutine == null)
            _aiUpdateCoroutine = StartCoroutine(AIUpdate());
    }

    // Find a repairable object within range.
    IEnumerator DetectRepairableObjects()
    {
        // Wait one frame to allow for state setup on Turrets.
        yield return null;
        
        int maxDetectionCount = _maxDetections;
        Collider[] _hitColliders = new Collider[maxDetectionCount];
        
        // Loop until externally cancelled.
        while (true)
        {
            // Get all close potential targets.
            Physics.OverlapSphereNonAlloc(transform.position, _detectionRadius, _hitColliders, _repairableLayers);
            
            // Loop through all potential targets (+1 for a null check).
            for(int i = 0; i <= maxDetectionCount; i++)
            {
                // If we haven't found a target and have looped through every detected collider, set the target to null.
                if (i == maxDetectionCount) {
                    _target = null;
                    break;
                }

                // If this collider hasn't been set, then return;
                if (_hitColliders[i] == null)
                    continue;

                // Check if the collider contains a Repairable component.
                if (_hitColliders[i].TryGetComponent<Repairable>(out Repairable repairable))
                {
                    // If this repairable is not able to be interacted with/repaired, continue.
                    if (repairable.GetInteractionAvailability() == false)
                        continue;

                    // Set the current target.
                    _target = repairable;
                    break;
                }
            }

            // Don't update every frame.
            yield return new WaitForSeconds(_detectionUpdateDelay);
        }
    }
    IEnumerator AIUpdate()
    {
        float distance = 0;
        bool inRange = false;

        // Loop until externally cancelled.
        while (true)
        {
            // Check if we have a target.
            if (_target != null)
            {
                // If the target cannot be interacted with for whatever reason (E.g. It has already been repaired), remove the target.
                if (_target.GetInteractionAvailability() == false)
                {
                    _target = null;
                    yield return new WaitForSeconds(_aiUpdateDelay);
                    continue;
                }

                
                distance = (_target.transform.position - transform.position).sqrMagnitude;


                // If we are close enough to repair the target, then do so.
                if (distance < (_repairRadius * _repairRadius))
                {
                    // Stop moving.
                    _agent.SetDestination(transform.position);
                    
                    // Start the interaction
                    _target.Interact(transform);
                    inRange = true;
                }
                else
                {
                    // Set the destination of the agent.
                    _agent.SetDestination(_target.transform.position);
                    
                    // We have left the range of repair, so cancel the repair.
                    if (inRange) {
                        inRange = false;
                        _target.OutOfRange(transform);
                    }
                }
            }
            else
            {
                // If we don't have a target, return to our default position.
                _agent.SetDestination(_defaultPosition);
            }


            yield return new WaitForSeconds(_aiUpdateDelay);
        }
    }


    private void OnDrawGizmosSelected()
    {
        // A Gizmo showing the detection radius.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        // A Gizmo showing the repair radius & default position/
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _repairRadius);
        Gizmos.DrawSphere(_defaultPosition, 0.25f);
    }
}
