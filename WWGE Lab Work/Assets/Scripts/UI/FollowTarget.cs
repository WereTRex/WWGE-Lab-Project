using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;

    private void Start() => transform.SetParent(WorldCanvasConnector.Instance.WorldCanvas);
    public void SetTarget(Transform target) => this._target = target;


    private void Update()
    {
        if (_target != null)
            transform.position = _target.position + _offset;
    }
}
