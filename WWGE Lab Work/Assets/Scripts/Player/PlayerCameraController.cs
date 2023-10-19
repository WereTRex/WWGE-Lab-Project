using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 100f;
    [SerializeField] private Transform _playerBody;

    private Vector2 _mouseInput;
    private float _xRotation;


    #region New Input System
    public void OnLook(InputAction.CallbackContext context) => _mouseInput = context.ReadValue<Vector2>();
    #endregion


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Vertical Rotation.
        _xRotation -= _mouseInput.y * _sensitivity * Time.deltaTime;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Rotate the Camera (Vertical Rotation) and PlayerBody (Horizontal Rotation).
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerBody.Rotate(Vector3.up * _mouseInput.x * _sensitivity * Time.deltaTime);
    }
}
