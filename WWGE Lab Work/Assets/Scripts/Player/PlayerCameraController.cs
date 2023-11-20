using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(Camera))]
public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Vector2 _defaultMouseSensitivity = new Vector2(25f, 30f);
    [SerializeField] private bool _defaultInvertMouseY = false;
    [SerializeField] private Vector2 _defaultGamepadSensitivity = new Vector2(100f, 100f);
    [SerializeField] private bool _defaultInvertGamepadY = false;

    [Space(5)]

    [SerializeField] private Transform _playerBody;
    private Camera _camera;

    private Vector2 _lookInput;
    private float _xRotation;

    private bool _useMouse = true;


    #region New Input System
    public void OnEnable() => InputUser.onChange += OnInputDeviceChanged;
    public void OnDisable() => InputUser.onChange -= OnInputDeviceChanged;
    


    private void OnInputDeviceChanged(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged)
        {
            Debug.Log(user.controlScheme.Value.name);

            if (user.controlScheme.Value.name.ToLower() == "mnk")
                _useMouse = true;
            else
                _useMouse = false;
        }
    }


    public void OnLook(InputAction.CallbackContext context) => _lookInput = context.ReadValue<Vector2>();
    #endregion




    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _camera = GetComponent<Camera>();

        InitializeValues();
    }

    // Get values from PlayerPrefs (FoV, Sensitivity, InvertY).
    private void InitializeValues()
    {
        PlayerManager.Instance.FieldOfView = _camera.fieldOfView;


        PlayerManager.Instance.MouseSensitivity = _defaultMouseSensitivity;
        PlayerManager.Instance.InvertMouseY = _defaultInvertMouseY;

        PlayerManager.Instance.GamepadSensitivity = _defaultGamepadSensitivity;
        PlayerManager.Instance.InvertGamepadY = _defaultInvertGamepadY;
    }


    private void Update()
    {
        // Vertical Rotation.
        if (_useMouse)
            _xRotation -= (PlayerManager.Instance.InvertMouseY ? -1f : 1f) * _lookInput.y * PlayerManager.Instance.MouseSensitivity.y * Time.deltaTime;
        else
            _xRotation -= (PlayerManager.Instance.InvertGamepadY ? -1f : 1f) * _lookInput.y * PlayerManager.Instance.GamepadSensitivity.y * Time.deltaTime;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Rotate the Camera (Vertical Rotation) and PlayerBody (Horizontal Rotation).
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        if (_useMouse)
            _playerBody.Rotate(Vector3.up * _lookInput.x * PlayerManager.Instance.MouseSensitivity.x * Time.deltaTime);
        else
            _playerBody.Rotate(Vector3.up * _lookInput.x * PlayerManager.Instance.GamepadSensitivity.x * Time.deltaTime);

        // Field of View (Can be made more efficient).
        _camera.fieldOfView = PlayerManager.Instance.FieldOfView;
    }
}
