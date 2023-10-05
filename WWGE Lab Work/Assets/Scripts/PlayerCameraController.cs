using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public float Sensitivity = 100f;
    public Transform PlayerBody;

    private float _xRotation;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Get Mouse Input.
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;

        // Vertical Rotation.
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Rotate the Camera (Vertical Rotation) and PlayerBody (Horizontal Rotation).
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        PlayerBody.Rotate(Vector3.up * mouseX);
    }
}
