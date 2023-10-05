using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _jumpForce = 10f;

    [Header("Gravity & Ground Check")]
    [SerializeField] private float _gravity = -9.81f;

    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    private bool _isGrounded;


    private CharacterController _controller;
    private Vector3 _velocity;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Player Movement.
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * y;
        _controller.Move(move * _speed * Time.deltaTime);


        // Gravity.
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        _velocity.y += _gravity * Time.deltaTime;
        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;
        

        // Jumping.
        if (Input.GetButtonDown("Jump") && _isGrounded)
            _velocity.y = _jumpForce;

        _controller.Move(_velocity * Time.deltaTime);
    }
}
