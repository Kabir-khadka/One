using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class AnimandMovement : MonoBehaviour
{
    [Header("Component References")]
    private PlayerInput playerInput;
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider capsuleCollider;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private float rotationFactorPerFrame = 9f;

    [Header("Jump Settings")]
    [SerializeField] private float maxJumpHeight = 3.0f;
    [SerializeField] private float maxJumpTime = 0.75f;
    [SerializeField] private float coyoteTime = 1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float fallMultiplier = 4.0f;
    [SerializeField] private LayerMask groundLayer;

    // Animator Hash IDs
    private int isWalkingHash;
    private int isRunningHash;
    private int isJumpingHash;
    private int jumpCountHash;

    // Movement state variables
    private Vector2 currentMovementInput;
    private bool isMovementPressed;
    private bool isRunPressed;
    private bool isJumpPressed;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // Jump state variables
    private bool isJumping = false;
    private bool isJumpAnimating = false;
    private int jumpCount = 0;
    private Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    private Dictionary<int, float> jumpGravities = new Dictionary<int, float>();
    private Coroutine currentJumpResetRoutine = null;

    // Physics variables
    private float gravity;
    private float groundedGravity = -0.05f;
    private Vector3 appliedMovement;

    private enum MovementState
    {
        Grounded,
        Jumping,
        Falling
    }
    private MovementState currentState;

    private void Awake()
    {
        // Get component references
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        if (playerInput == null || rb == null || animator == null || capsuleCollider == null)
        {
            Debug.LogError("Required components missing from GameObject.");
            enabled = false;
            return;
        }

        // Set up animator parameters
        SetupAnimatorParameters();

        // Set up input callbacks
        SetupInputCallbacks();

        // Initialize jump variables
        SetupJumpVariables();
    }

    private void SetupAnimatorParameters()
    {
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        jumpCountHash = Animator.StringToHash("jumpCount");
    }

    private void SetupInputCallbacks()
    {
        playerInput.actions["Move"].started += onMovementInput;
        playerInput.actions["Move"].canceled += onMovementInput;
        playerInput.actions["Move"].performed += onMovementInput;

        playerInput.actions["Sprint"].started += onSprint;
        playerInput.actions["Sprint"].canceled += onSprint;

        playerInput.actions["Jump"].started += onJump;
        playerInput.actions["Jump"].canceled += onJump;
    }

    private void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);

        // Calculate different jump heights and velocities
        for (int i = 1; i <= 3; i++)
        {
            float heightModifier = i == 1 ? 0 : (i == 2 ? 2 : 4);
            float timeModifier = i == 1 ? 1 : (i == 2 ? 1.25f : 1.5f);

            float modifiedHeight = maxJumpHeight + heightModifier;
            float modifiedTimeToApex = timeToApex * timeModifier;

            float jumpGravity = (-2 * modifiedHeight) / Mathf.Pow(modifiedTimeToApex, 2);
            float initialVelocity = (2 * modifiedHeight) / modifiedTimeToApex;

            initialJumpVelocities[i] = initialVelocity;
            jumpGravities[i] = jumpGravity;
        }

        // Add base gravity
        jumpGravities[0] = gravity;
    }

    private void HandleJump()
    {
        // Update coyote time
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Update jump buffer
        if (isJumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Handle jump execution
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
        {
            if (currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);
                currentJumpResetRoutine = null;
            }

            // Set jump states and animate
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;

            // Increment and wrap jump count
            jumpCount = (jumpCount % 3) + 1;
            animator.SetInteger(jumpCountHash, jumpCount);

            // Apply jump force
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, initialJumpVelocities[jumpCount], rb.linearVelocity.z);

            // Reset jump buffer
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // Handle jump release (variable jump height)
        if (!isJumpPressed && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
        }

        // Handle landing
        if (IsGrounded() && rb.linearVelocity.y <= 0)
        {
            if (isJumping)
            {
                isJumping = false;
                animator.SetBool(isJumpingHash, false);

                if (currentJumpResetRoutine == null)
                {
                    currentJumpResetRoutine = StartCoroutine(JumpResetRoutine());
                }
            }
        }
    }

    private bool IsGrounded()
    {
        float rayLength = 0.7f;
        Vector3 characterBottom = transform.position + Vector3.up * 0.1f;
        float radius = capsuleCollider.radius;

        // Cast rays in a circle around the character
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8;
            Vector3 rayStart = characterBottom + new Vector3(
                Mathf.Cos(angle) * radius * 0.8f,
                0,
                Mathf.Sin(angle) * radius * 0.8f
            );

            if (Physics.Raycast(rayStart, Vector3.down, rayLength, groundLayer))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        jumpCount = 0;
        animator.SetInteger(jumpCountHash, jumpCount);
        currentJumpResetRoutine = null;
    }

    private void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    private void onSprint(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    private void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    // Also update the cleanup in OnDisable to prevent memory leaks
    private void OnDisable()
    {
        // Remove the callbacks when disabled
        if (playerInput != null)
        {
            playerInput.actions["Move"].started -= onMovementInput;
            playerInput.actions["Move"].canceled -= onMovementInput;
            playerInput.actions["Move"].performed -= onMovementInput;

            playerInput.actions["Sprint"].started -= onSprint;
            playerInput.actions["Sprint"].canceled -= onSprint;

            playerInput.actions["Jump"].started -= onJump;
            playerInput.actions["Jump"].canceled -= onJump;

            playerInput.actions.Disable();
        }
    }

    private void HandleMovement()
    {
        if (isMovementPressed)
        {
            Vector3 movementDirection = CalculateMovementDirection();
            float targetSpeed = isRunPressed ? runSpeed : walkSpeed;

            // Calculate target velocity
            Vector3 targetVelocity = movementDirection * targetSpeed;

            // Smoothly interpolate to target velocity
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 newVelocity = Vector3.Lerp(
                currentHorizontalVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );

            // Apply movement force
            Vector3 velocityChange = newVelocity - currentHorizontalVelocity;
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            // Apply deceleration
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(-currentHorizontalVelocity * deceleration, ForceMode.Acceleration);
        }
    }

    private Vector3 CalculateMovementDirection()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        return (cameraRight * currentMovementInput.x + cameraForward * currentMovementInput.y).normalized;
    }

    private void HandleRotation()
    {
        if (isMovementPressed)
        {
            Vector3 movementDirection = CalculateMovementDirection();
            if (movementDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationFactorPerFrame * Time.deltaTime
                );
            }
        }
    }

    private void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if ((isMovementPressed && isRunPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }

    private void HandleGravity()
    {
        bool isFalling = rb.linearVelocity.y <= 0.0f || !isJumpPressed;

        if (isJumping)
        {
            // Apply appropriate jump gravity based on jump count
            float currentGravity = jumpGravities[Mathf.Min(jumpCount, 3)];
            rb.AddForce(Vector3.up * currentGravity, ForceMode.Acceleration);
        }
        else if (isFalling && !IsGrounded())
        {
            // Apply increased gravity while falling
            rb.AddForce(Vector3.up * gravity * fallMultiplier, ForceMode.Acceleration);
        }
        else if (IsGrounded())
        {
            // Apply small downward force when grounded
            rb.AddForce(Vector3.up * groundedGravity, ForceMode.Acceleration);
        }
    }

    private void UpdateMovementState()
    {
        if (IsGrounded())
        {
            currentState = MovementState.Grounded;
        }
        else if (rb.linearVelocity.y > 0)
        {
            currentState = MovementState.Jumping;
        }
        else
        {
            currentState = MovementState.Falling;
        }
    }

    private void Update()
    {
        UpdateMovementState();
        HandleMovement();
        HandleRotation();
        HandleAnimation();
        HandleGravity();
        HandleJump();
    }

    private void OnEnable()
    {
        playerInput.actions.Enable();
    }

 

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw ground detection rays
        if (capsuleCollider != null)
        {
            float rayLength = 0.7f;
            Vector3 characterBottom = transform.position + Vector3.up * 0.1f;
            float radius = capsuleCollider.radius;

            Gizmos.color = Color.green;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8;
                Vector3 rayStart = characterBottom + new Vector3(
                    Mathf.Cos(angle) * radius * 0.8f,
                    0,
                    Mathf.Sin(angle) * radius * 0.8f
                );

                Gizmos.DrawLine(rayStart, rayStart + Vector3.down * rayLength);
            }
        }
    }
#endif
}