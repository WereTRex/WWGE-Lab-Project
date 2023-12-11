using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A script that forces an object to follow a target at a rigid offset from the world canvas.</summary>
public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;

    [Space(5)]

    [Tooltip("Should this object destroy itself if it loses its target?")] [SerializeField] private bool _destroyOnNull = true;

    private void Start() => transform.SetParent(WorldCanvasConnector.Instance.WorldCanvas); // Set this object's parent to the World Canvas.
    public void SetTarget(Transform target) => this._target = target; // Set the current target


    private void Update()
    {
        // Follow the target.
        if (_target != null)
            transform.position = _target.position + _offset;
        // If we have no target, then check whether we should destroy ourself.
        else if (_destroyOnNull)
            Destroy(this.gameObject);
    }
}
