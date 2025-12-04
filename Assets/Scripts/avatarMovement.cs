using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

public class avatarMovement : MonoBehaviour
{
    [Space]
    [Header("== Componentes ==")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D groundCollider;
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private Animator animator;

    [Space]
    [Header("== Movimentação ==")]
    [SerializeField] private float moveSpeed  = 5f;
    [SerializeField] private float jumpForce  = 5f;
    [SerializeField] private float stairSpeed = 5f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask stairlayer;
    [SerializeField] private bool isFacingRight = true;

    private bool isDead = false;
    private bool isJumping = false;
    private bool onStair = false;
    private float moveInput;
    private int hasJumped = 0;  

    [Space]
    [Header("== Eventos ==")]
    public UnityEvent OnFlip;
    public UnityEvent OnJump;
    public UnityEvent OnLanding;
    void Start()
    {
        
    }

    void Update()
    {
        IsGrounded();
        IsOnStair();

        if (!isDead)
        {
            HorizontalMovement();
            Flip();
            VerticalMovement();
        }
    }

    void HorizontalMovement()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocityY);
        if(Math.Abs(moveInput) > 0)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
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
            OnFlip?.Invoke();
        }
    }

    void IsGrounded()
    {
        if (groundCollider.IsTouchingLayers(wallLayer) || groundCollider.IsTouchingLayers(groundLayer) || groundCollider.IsTouchingLayers(obstacleLayer))
        {
            isJumping = false;
        }
        else
        {
            hasJumped = 1;
            isJumping = true;
        }
    }

    void IsOnStair()
    {
        if (playerCollider.IsTouchingLayers(stairlayer))
        {
            onStair = true;
        }
        else
        {
            onStair = false;
        }
    }

    void VerticalMovement()
    {
        if (!isJumping && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce * 1.5f);
            OnJump?.Invoke();
            animator.SetBool("isJumping", true);
        }
        if (!isJumping && rb.linearVelocityY <= 0)
        {
            if(hasJumped == 1)
            {
                OnLanding?.Invoke();
                hasJumped = 0;
            }
            animator.SetBool("isJumping", false);
        }

        if(onStair && Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, stairSpeed);
            animator.SetBool("isJumping", false);
        }
    }

    void OnDisable()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
    }

    void OnEnable()
    {
        isDead = false;
    }
}
