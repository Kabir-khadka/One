using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Unity.VisualScripting;
using static FloatingJoystick;
using static UnityEngine.AudioSettings;
using UnityEngine.Rendering;

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
    [SerializeField] private float maxJumpHeight = 8.0f;
    [SerializeField] private float maxJumpTime = 1f;
    [SerializeField] private float coyoteTime = 1f;
    [SerializeField] private float jumpBufferTime = 0.8f;
    [SerializeField] private float fallMultiplier = 4.0f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Mobile Controls")]
    [SerializeField] private bool useMobileControls = false;
    [SerializeField] private GameObject mobileControlsCanvas;
    [SerializeField] private FloatingJoystick movementJoystick;  // Reference to your FloatingJoystick
    [SerializeField] private Button jumpButton;
   

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
    private bool wasJumpPressed = false; // Track previous frame state
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

        if (rb == null || animator == null || capsuleCollider == null)
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

        // Initialize mobile controls if needed
        InitializeMobileControls();

        Debug.Log("Mobile Controls Enabled: " + useMobileControls);
        Debug.Log("Canvas Reference: " + (mobileControlsCanvas != null ? mobileControlsCanvas.name : "NULL"));
        Debug.Log("Canvas Active: " + (mobileControlsCanvas != null ? mobileControlsCanvas.activeSelf : false));
        Debug.Log("Joystick Reference: " + (movementJoystick != null ? movementJoystick.name : "NULL"));
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
        if (!useMobileControls && playerInput != null)
        {
            playerInput.actions["Move"].started += onMovementInput;
            playerInput.actions["Move"].canceled += onMovementInput;
            playerInput.actions["Move"].performed += onMovementInput;
            playerInput.actions["Jump"].started += onJump;
            playerInput.actions["Jump"].canceled += onJump;
            Debug.Log("Mobile controls are DISABLED, regular controls enabled");
        }
        else
        {
            Debug.Log("Mobile controls are ENABLED");
        }
    }

    private void InitializeMobileControls()
    {
        // Setup mobile controls if needed
        if (useMobileControls)
        {
            // Make sure mobile canvas is active
            if (mobileControlsCanvas != null)
            {
                mobileControlsCanvas.SetActive(true);
            }
            else
            {
                Debug.LogError("Mobile Controls Canvas is not assigned!");
            }

            // Check if joystick is assigned
            if (movementJoystick == null)
            {
                Debug.LogError("Movement Joystick is not assigned!");
            }

            // Setup jump button listener
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(() => { isJumpPressed = true; });
            }


            // Disable regular input system if using mobile controls
            if (playerInput != null)
            {
                playerInput.actions.Disable();
            }
        }
        else
        {
            // Hide mobile controls if not using them
            if (mobileControlsCanvas != null)
            {
                mobileControlsCanvas.SetActive(false);
            }
        }
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

    public void HandleJump()
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

        // Process jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
        {
            if (currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);
                currentJumpResetRoutine = null;
            }

            // Set jump states
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;

            // Increment and wrap jump count
            jumpCount = (jumpCount % 3) + 1;
            animator.SetInteger(jumpCountHash, jumpCount);

            // Apply jump force
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, initialJumpVelocities[jumpCount], rb.linearVelocity.z);

            // Reset jump buffer & coyote time
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            // Reset jump input
            isJumpPressed = false;
        }

        // Adjust variable jump height to be more natural
        if (!isJumpPressed && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 1f, rb.linearVelocity.z); // Less aggressive dampening
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


    private void onJump(InputAction.CallbackContext context)
    {
        if (context.started) // Trigger only when the key is first pressed
        {
            isJumpPressed = true;
        }
    }

    // Additional method for UI Button click (to be connected in OnClick)
    public void OnJumpButtonClicked()
    {
        isJumpPressed = true;
    }

    // Handle mobile joystick input
    private void HandleMobileInput()
    {
        if (!useMobileControls || movementJoystick == null) return;

        // Get input from joystick
        Vector2 joystickInput = new Vector2(movementJoystick.Horizontal, movementJoystick.Vertical);
        Debug.Log("Joystick Input: " + joystickInput);

        // Update movement variables
        currentMovementInput = joystickInput;
        isMovementPressed = joystickInput.magnitude > 0.1f;  // Add small deadzone
    }

    private void HandleMovement()
    {
        if (useMobileControls)
        {
            HandleMobileInput();  // Ensure joystick input is read every frame
        }

        if (isMovementPressed)
        {
            Vector3 movementDirection = CalculateMovementDirection();

            // Calculate target speed based on joystick magnitude
            float targetSpeed = Mathf.Lerp(walkSpeed, runSpeed, currentMovementInput.magnitude);

            // Calculate target velocity
            Vector3 targetVelocity = movementDirection * targetSpeed;

            // Smooth velocity change
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 newVelocity = Vector3.Lerp(
                currentHorizontalVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );

            // Apply movement force
            Vector3 velocityChange = newVelocity - currentHorizontalVelocity;
            rb.AddForce(velocityChange, ForceMode.VelocityChange);

            // Update animations
            animator.SetBool(isWalkingHash, currentMovementInput.magnitude > 0.1f);
            animator.SetBool(isRunningHash, currentMovementInput.magnitude > 0.5f);
        }
        else
        {
            // Decelerate character smoothly
            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(-currentHorizontalVelocity * deceleration, ForceMode.Acceleration);

            // Stop animations
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
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
        // Handle mobile input if enabled
        if (useMobileControls)
        {
            HandleMobileInput();
        }

        UpdateMovementState();
        HandleMovement();
        HandleRotation();
        HandleGravity();
        HandleJump();
    }

    private void OnEnable()
    {
        if (playerInput != null && !useMobileControls)
        {
            playerInput.actions.Enable();
        }
    }

    private void OnDisable()
    {
        // Remove the callbacks when disabled
        if (playerInput != null)
        {
            playerInput.actions["Move"].started -= onMovementInput;
            playerInput.actions["Move"].canceled -= onMovementInput;
            playerInput.actions["Move"].performed -= onMovementInput;

            

            playerInput.actions["Jump"].started -= onJump;
            playerInput.actions["Jump"].canceled -= onJump;

            playerInput.actions.Disable();
        }



        // Clean up mobile control listeners
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
        }


        
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