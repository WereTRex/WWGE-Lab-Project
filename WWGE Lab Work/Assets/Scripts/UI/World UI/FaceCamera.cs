using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A class that forces the attached GameObject to face the assigned camera.</summary>
public class FaceCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private void Awake()
    {
        // If we have not assigned the camera to face, grab the main camera.
        if (_camera == null)
            _camera = Camera.main;
    }
    public void SetCamera(Camera camera) => this._camera = camera; // Set the camera.


    private void Update()
    {
        // If we have a camera, face it.
        if (_camera != null)
            transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
    }
}
