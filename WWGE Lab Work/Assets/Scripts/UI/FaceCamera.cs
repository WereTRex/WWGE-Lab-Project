using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }
    public void SetCamera(Camera camera) => this._camera = camera;


    private void Update()
    {
        if (_camera != null)
            transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
    }
}
