using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

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
        if (_detectObjectsCoroutine == null)
            _detectObjectsCoroutine = StartCoroutine(DetectRepairableObjects());
        if (_aiUpdateCoroutine == null)
            _aiUpdateCoroutine = StartCoroutine(AIUpdate());
    }

    IEnumerator DetectRepairableObjects()
    {
        // Wait one frame to allow for state setup on Turrets.
        yield return null;
        
        int maxDetectionCount = _maxDetections;
        Collider[] _hitColliders = new Collider[maxDetectionCount];
        while (true)
        {
            // Get a target.
            Physics.OverlapSphereNonAlloc(transform.position, _detectionRadius, _hitColliders, _repairableLayers);
            for(int i = 0; i <= maxDetectionCount; i++)
            {
                // If we haven't found a target and have looped through every detected collider, set the target to null;
                if (i == maxDetectionCount) {
                    _target = null;
                    break;
                }

                // If this collider hasn't been set, then return;
                if (_hitColliders[i] == null)
                    continue;

                // If the collider contains a Repairable component, and that repairable component is available, then set that to the target.
                if (_hitColliders[i].TryGetComponent<Repairable>(out Repairable repairable)) {
                    if (repairable.GetInteractionAvailability() == false)
                        continue;

                    _target = repairable;
                    break;
                }
            }

            yield return new WaitForSeconds(_detectionUpdateDelay);
        }
    }
    IEnumerator AIUpdate()
    {
        float distance = 0;
        bool inRange = false;
        while (true)
        {
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
                    _agent.SetDestination(transform.position);
                    _target.Interact(transform);
                    inRange = true;
                } else {
                    // Set the destination of the agent.
                    _agent.SetDestination(_target.transform.position);
                    
                    // We have left the range of repair, so cancel the repair.
                    if (inRange) {
                        inRange = false;
                        _target.OutOfRange(transform);
                    }
                }
            } else {
                _agent.SetDestination(_defaultPosition);
            }


            yield return new WaitForSeconds(_aiUpdateDelay);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _repairRadius);
        Gizmos.DrawSphere(_defaultPosition, 0.25f);
    }
}
