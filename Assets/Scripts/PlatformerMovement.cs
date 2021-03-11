using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlatformerMovement : NetworkBehaviour
{
    //Input Field
    private PlayerInput input;

    //Movement Field
    [SerializeField] private float moveSpeed;
    private Vector2 currentSpeed;

    //External Force
    [SerializeField] private float externalForceXDecay;
    private float currentForceX;

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

    //Audio
    public AudioSource audioSource;
    //public AudioClip Footsteps;
    public AudioClip JumpSound;

    //private void Awake()
    //{
    //    controls = new Controls();
    //    playerInput = GetComponent<PlayerInput>();
    //}

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        input = GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        sprite = GetComponent<SpriteRenderer>() ? GetComponent<SpriteRenderer>() : null;

        initGravity = rb.gravityScale;

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority) return;
        if (!input) return;

        GroundCheck();
        PlatformCheck();

        FlipDirection();
        
        OneWayPlatformInput();

        Movement();
        ForceDecay();
        CalculateVelocity();
    }

    private void OneWayPlatformInput()
    {
        if (onPlatform)
        {
            if (input.movementInput.y < 0)
            {
                StartCoroutine(ResetPlatformCheck());
            }
        }

        platformCollider.enabled = !jumpingThroughPlatform;
    }

    private void Movement()
    {
        currentSpeed = new Vector2(input.movementInput.x * moveSpeed, rb.velocity.y);
    }

    private void CalculateVelocity()
    {
        rb.velocity = currentSpeed + new Vector2(currentForceX, 0);
    }

    private void FlipDirection()
    {
        if (input.movementInput.x < 0)
        {
            faceLeft = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        if (input.movementInput.x > 0)
        {
            faceLeft = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void JumpCall(CallbackContext ctx)
    {
        if (!hasAuthority) return;
        Jump();
    }

    private void Jump()
    {
        if (!isGrounded) return;
        float yVelo = jumpForce;
        rb.velocity = new Vector2(rb.velocity.x, yVelo);
        audioSource.PlayOneShot(JumpSound);
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
    }

    public void AddExternalForceX(float force)
    {
        currentForceX = force;
    }

    private void ForceDecay()
    {
        currentForceX = Mathf.Lerp(currentForceX, 0, (-input.movementInput.x + externalForceXDecay) * Time.deltaTime);

        if (Mathf.Abs(currentForceX) < 0.1) currentForceX = 0;
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
