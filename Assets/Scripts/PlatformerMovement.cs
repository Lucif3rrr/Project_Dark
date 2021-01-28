﻿using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlatformerMovement : NetworkBehaviour
{
    //Input Field
    private PlayerInput playerInput;
    private Controls controls;

    //Movement Field
    [SerializeField] private float moveSpeed;
    private float moveX;

    //Direction Field
    public bool faceLeft;
    [SerializeField] private SpriteRenderer playerSprite;

    //JumpField
    [SerializeField] private float jumpForce;
    
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayer;
    public bool isGrounded;
    [SerializeField] private float groundDelay;
    private float rememberGroundTime = 0;

    //Ladder Field
    public bool onLadder;
    public bool ladderBelow;
    [SerializeField] private float climbSpeed = 1.0f;
    private float initGravity;

    [SerializeField] private Vector3 platformCheckOffset;
    [SerializeField] private float platformCheckRadius;
    [SerializeField] private LayerMask platformLayer;

    //OneWayPlatform Field
    public bool onPlatform;
    public bool jumpingThroughPlatform = false;
    public float jumpThroughDelay;

    //Misc
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    [SerializeField] private Collider2D environmentCollider;
    [SerializeField] private Collider2D platformCollider;
    [SerializeField] private Collider2D hitCollider;

    //Network
    private NetworkIdentity net;

    private void Awake()
    {
        controls = new Controls();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if(playerInput.controller == InputController.KEYBOARD)
        {
            if (playerInput.playerNumberOnDevice == 0)
            {
                controls.Player1.Jump.performed += Jump;
                controls.Player1.Enable();   
            }
            else
            {
                controls.Player2.Jump.performed += Jump;
                controls.Player2.Enable();
            }
        }
    }

    private void OnDisable()
    {
        if (playerInput.controller == InputController.KEYBOARD)
        {
            if (playerInput.playerNumberOnDevice == 0)
            {
                controls.Player1.Disable();
            }
            else
            {
                controls.Player2.Disable();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        sprite = GetComponent<SpriteRenderer>() ? GetComponent<SpriteRenderer>() : null;

        initGravity = rb.gravityScale;

        net = GetComponent<NetworkIdentity>() ? GetComponent<NetworkIdentity>() : null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!net.hasAuthority) return;

        GroundCheck();
        PlatformCheck();

        //Movement Input
        MovementInput();
        FlipDirection();
        //JumpInput();
        LadderInput();
        OneWayPlatformInput();

        Movement();
    }

    private void OneWayPlatformInput()
    {
        if (onPlatform)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                StartCoroutine(ResetPlatformCheck());
                platformCollider.enabled = false;
            }
        }
    }

    private void MovementInput()
    {
        Vector2 movementInput = new Vector2();
        var movement = new Vector2();
        if (playerInput.controller == InputController.KEYBOARD)
        {
            if(playerInput.playerNumberOnDevice == 0)
            {
                movementInput = controls.Player1.Movement.ReadValue<Vector2>();
            }
            else
            {
                movementInput = controls.Player2.Movement.ReadValue<Vector2>();
            }
        }

        movement.x = movementInput.x;

        moveX = movement.x * moveSpeed; 
    }

  
    private void Movement()
    {
        float xVelo = moveX * Time.deltaTime;
        rb.velocity = new Vector2(moveX, rb.velocity.y);
    }

    private void FlipDirection()
    {
        if (moveX < 0)
        {
            faceLeft = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        if (moveX > 0)
        {
            faceLeft = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void JumpInput()
    {
        if (onLadder) return;
        if (playerInput.controller == InputController.KEYBOARD)
        {
            if (playerInput.playerNumberOnDevice == 0)
            {
                if (controls.Player1.Jump.triggered)
                {
                    print("JUMP");
                }
            }
            else
            {
                if (controls.Player2.Jump.triggered)
                {
                    print("JUMP");
                }
            }
        }
    }

    private void LadderInput()
    {
        if (onLadder)
        {
            platformCollider.enabled = false;
            if (Input.GetKey(KeyCode.W))
            {
                rb.velocity = new Vector2(rb.velocity.x, climbSpeed);
            }
            else if (Input.GetKey(KeyCode.S) && ladderBelow)
            {
                rb.velocity = new Vector2(rb.velocity.x, -climbSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }

            rb.gravityScale = 0;
            
        }
        else
        {
            rb.gravityScale = initGravity;
        }

        if (ladderBelow)
        {
            if (Input.GetKey(KeyCode.S) && !onLadder)
            {
                platformCollider.enabled = false;
                rb.velocity = new Vector2(rb.velocity.x, -climbSpeed);
            }
        }
    }

    public void Jump(CallbackContext ctx)
    {
        if (!isGrounded) return;
        float yVelo = jumpForce;
        rb.velocity = new Vector2(rb.velocity.x, yVelo);
    }

    private void PlatformCheck()
    {
        RaycastHit2D platform = Physics2D.Raycast(transform.position + platformCheckOffset, -transform.up, platformCheckRadius, platformLayer);

        if (platform)
        {
            onPlatform = true;
        }
        else
        {
            onPlatform = false;
        }

    }

    private void GroundCheck()
    {
        RaycastHit2D ground = Physics2D.Raycast(transform.position + groundCheckOffset, -transform.up, groundCheckRadius, groundLayer);
        
        if (ground)
        {
            isGrounded = true;
            rememberGroundTime = groundDelay;
        }
        else
        {
            if(rememberGroundTime > 0)
            {
                rememberGroundTime -= Time.deltaTime;
            }

            if(rememberGroundTime <= 0)
            {
                isGrounded = false;
            }
        }

        if (!jumpingThroughPlatform)
        {
            platformCollider.enabled = isGrounded;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position + groundCheckOffset, transform.position + groundCheckOffset + -transform.up * groundCheckRadius);
    }

    IEnumerator ResetPlatformCheck()
    {
        jumpingThroughPlatform = true;
        yield return new WaitForSeconds(jumpThroughDelay);
        jumpingThroughPlatform = false;

    }
}
