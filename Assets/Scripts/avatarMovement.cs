using UnityEditor.Animations;
using UnityEngine;
using System;

public class avatarMovement : MonoBehaviour
{
    [Space]
    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D collider;
    [SerializeField] private Animator animator;

    [Space]
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isFacingRight = true;

    private bool isJumping = false;
    private float moveInput;

    void Start()
    {
        
    }

    void Update()
    {
        HorizontalMovement();
        Flip();
        IsGrounded();
        VerticalMovement();
    }

    void FixedUpate()
    {

    }

    void HorizontalMovement()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocityY);
        if(Math.Abs(moveInput) > 0)
        {
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }
    }

    void Flip()
    {
        if (isFacingRight && moveInput < 0 || !isFacingRight && moveInput > 0) 
        {
            isFacingRight = !isFacingRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }
    }

    void IsGrounded()
    {
        if (collider.IsTouchingLayers(groundLayer))
        {
            isJumping = false;
        }
        else
        {
            isJumping = true;
        }
    }

    void VerticalMovement()
    {
        if (!isJumping && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce * 1.5f);
            animator.SetBool("Jumping", true);
        }
        if (!isJumping && rb.linearVelocityY <= 0)
        {
            animator.SetBool("Jumping", false);
        }
    }
}
