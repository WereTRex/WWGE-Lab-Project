using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Player.States;

public class PlayerController : MonoBehaviour
{
    private CharacterController _controller;


    [Header("Movement")]
    [SerializeField] private float _speed = 10f;
    private Vector2 _movementInput;


    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 10f;
    private bool _isJumpPressed = false;


    [Header("Gravity & Ground Check")]
    [SerializeField] private float _gravity = -9.81f;

    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    private bool _isGrounded;



    private StateMachine _stateMachine;
    private void Awake()
    {
        #region Setup State Machine
        _stateMachine = new StateMachine();

        // Root States.
        var grounded = new Grounded();
        var jumping = new Jumping();

        // Sub States
        var idle = new Idle();
        var walking = new Walking();
        var running = new Running();


        // Transitions.


        // Set the initial state.


        #region Transition Conditions.

        #endregion
        #endregion
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }


    #region Input
    public void OnMovementInput(InputAction.CallbackContext context) => _movementInput = context.ReadValue<Vector2>();
    public void OnJumpPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Jump!");
    }
    #endregion


    private void Update()
    {
        
    }
}
