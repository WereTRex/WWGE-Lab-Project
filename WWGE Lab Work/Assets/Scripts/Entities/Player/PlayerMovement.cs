using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _jumpForce = 10f;
    private Vector2 _movementInput;

    [Header("Gravity & Ground Check")]
    [SerializeField] private float _gravity = -9.81f;

    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    private bool _isGrounded;


    private CharacterController _controller;
    private Vector3 _velocity;


    #region New Input System
    public void OnMove(InputAction.CallbackContext context) => _movementInput = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded)
            Jump();
    }
    #endregion


    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;
        

        // Player Movement.
        Vector3 move = transform.right * _movementInput.x + transform.forward * _movementInput.y;
        _controller.Move(move * _speed * Time.deltaTime);


        // Gravity.
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        _velocity.y += _gravity * Time.deltaTime;
        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        _controller.Move(_velocity * Time.deltaTime);
    }


    private void Jump()
    {
        _velocity.y = _jumpForce;
    }
}
