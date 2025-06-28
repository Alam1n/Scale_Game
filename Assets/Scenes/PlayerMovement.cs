using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;
    public float slideSpeed = 20f;
    public float crouchSpeed = 3.5f;
    public float airMultiplier = 0.4f;
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public bool resetYVelocityOnJump = true;

    // Remove the duplicate definition of crouchSpeed under the "Crouching" header.  
    // The "Movement Settings" header already contains the definition for crouchSpeed.  

    [Header("Crouching")]
    public float crouchYScale = 0.5f;

    [Header("Ground Check")]    
    public float playerHeight = 2f;
    public LayerMask groundMask = 1;
    public float groundDrag = 5f;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 40f;

    [Header("References")]
    public Transform orientation;
    public Transform cameraPosition;

    // Input
    private Vector2 movementInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool crouchInput;
    private PlayerInputActions inputActions;

    // Movement
    private Vector3 moveDirection;
    private Rigidbody rb;
    private float currentSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    // Ground detection
    private bool grounded;
    private bool exitingSlope;

    // Jumping
    private bool readyToJump = true;

    // Crouching
    private float startYScale;
    private bool isCrouching;

    // Slope handling
    private RaycastHit slopeHit;

    // States
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air
    }

    void Awake()
    {
        inputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();

        // Movement input
        inputActions.Player.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Movement.canceled += ctx => movementInput = Vector2.zero;

        // Jump input
        inputActions.Player.Jump.performed += ctx => jumpInput = true;
        inputActions.Player.Jump.canceled += ctx => jumpInput = false;

        // Sprint input
        inputActions.Player.Sprint.performed += ctx => sprintInput = true;
        inputActions.Player.Sprint.canceled += ctx => sprintInput = false;

        // Crouch input
        inputActions.Player.Crouch.performed += ctx => crouchInput = true;
        inputActions.Player.Crouch.canceled += ctx => crouchInput = false;
    }

    void OnDisable()
    {
        inputActions.Player.Movement.performed -= ctx => movementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Movement.canceled -= ctx => movementInput = Vector2.zero;

        inputActions.Player.Jump.performed -= ctx => jumpInput = true;
        inputActions.Player.Jump.canceled -= ctx => jumpInput = false;

        inputActions.Player.Sprint.performed -= ctx => sprintInput = true;
        inputActions.Player.Sprint.canceled -= ctx => sprintInput = false;

        inputActions.Player.Crouch.performed -= ctx => crouchInput = true;
        inputActions.Player.Crouch.canceled -= ctx => crouchInput = false;

        inputActions.Player.Disable();
    }

    void Update()
    {
        GroundCheck();
        HandleInput();
        StateHandler();
        SpeedControl();
        HandleDrag();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void HandleInput()
    {
        // Handle jumping
        if (jumpInput && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Handle crouching
        if (crouchInput && grounded)
        {
            StartCrouch();
        }
        else if (!crouchInput)
        {
            StopCrouch();
        }
    }

    void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);

        // Visual debug (remove in production)
        Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), grounded ? Color.green : Color.red);
    }

    void StateHandler()
    {
        // Crouching
        if (crouchInput && grounded)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // Sprinting
        else if (grounded && sprintInput)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Air
        else
        {
            state = MovementState.air;
        }

        // Check if desired move speed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && currentSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            currentSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    System.Collections.IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - currentSpeed);
        float startValue = currentSpeed;

        while (time < difference)
        {
            currentSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        currentSpeed = desiredMoveSpeed;
    }

    void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;

        // On slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * currentSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // On ground
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        }
        // In air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    void SpeedControl()
    {
        // Limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > currentSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
        }
        // Limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Limit velocity if needed
            if (flatVel.magnitude > currentSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * currentSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    void HandleDrag()
    {
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    void Jump()
    {
        exitingSlope = true;

        // Reset y velocity
        if (resetYVelocityOnJump)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    void StartCrouch()
    {
        isCrouching = true;
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    void StopCrouch()
    {
        if (isCrouching)
        {
            isCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}