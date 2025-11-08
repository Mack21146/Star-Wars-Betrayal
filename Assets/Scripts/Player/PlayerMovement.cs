using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float dodgeSpeed = 4f;
    [SerializeField] private float dodgeDuration = 0.5f;
    private bool isRunning = false;
    private bool isDodging = false;

    private Vector2 moveInput;
    private Rigidbody2D rb;

    private Vector2 movementDirection;
    private Vector2 lastMoveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDodging)
        {
            return;
        }

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput.normalized;
        }

        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        rb.velocity = moveInput * currentSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isRunning = true;
            moveSpeed = runSpeed;
        }
        else if (context.canceled)
        {
            isRunning = false;
            moveSpeed = 2f;
        }
    }

    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.started && !isDodging)
        {
            StartCoroutine(DodgeCoroutine());
        }
    }

    private IEnumerator DodgeCoroutine()
    {
        if(lastMoveDirection == Vector2.zero)
        {
            yield break;
        }

        isDodging = true;

        float timer = 0;

        while(timer < dodgeDuration)
        {
            rb.velocity = lastMoveDirection * dodgeSpeed;
            timer += Time.deltaTime;
            yield return null;
        }
        
        isDodging = false;
    }
}
