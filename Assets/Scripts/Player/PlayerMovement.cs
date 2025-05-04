using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private int walkSpeed;
    [SerializeField] private int runSpeed;

    Vector2 moveInput;

    Rigidbody2D rb;
    Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput.x, moveInput.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if(moveInput.x > 0)
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("facingRight", true);
            anim.SetBool("facingDown", false);
            anim.SetBool("facingLeft", false);
            anim.SetBool("facingUp", false);
        }

        else
        {
            anim.SetBool("isWalking", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
