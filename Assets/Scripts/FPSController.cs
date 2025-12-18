using UnityEngine;

// This script requires a CharacterController component on the same GameObject
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    // --- SCENE REFERENCES ---
    [Header("Hierarchy References")]
    [Tooltip("The camera used for looking around (Child of the Player).")]
    public Transform playerCamera;
    [Tooltip("The empty GameObject where projectiles spawn (End of the barrel).")]
    public Transform firePoint;
    //[Tooltip("The projectile prefab to be instantiated (Must have a Rigidbody).")]
   // public GameObject projectilePrefab;

    // --- MOVEMENT SETTINGS ---
    [Header("Movement Settings")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public float jumpHeight = 1.5f;
    public float gravity = 20.0f;

    // Internal state variables
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float currentSpeed;

    // --- CAMERA LOOK SETTINGS ---
    [Header("Look Settings")]
    public float mouseSensitivity = 2.0f;
    [Tooltip("The vertical angle limit for looking up/down (e.g., -90 to 90).")]
    public float maxLookAngle = 90.0f;
    private float rotationX = 0; // Stores vertical rotation

 

    void Start()
    {
        // Get the CharacterController component
        characterController = GetComponent<CharacterController>();

        // Lock and hide the cursor for FPV control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        // Check for essential references
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera not assigned in the Inspector!");
            enabled = false; // Disable the script if essential components are missing
        }
    }

    void Update()
    {
        HandleCameraLook();
        HandleMovementInput();
        HandleJump();
        //HandleShooting();
    }

    void FixedUpdate()
    {
        // Apply movement using the CharacterController
        // We use Time.deltaTime here, but CharacterController can also work in Update()
        characterController.Move(moveDirection * Time.deltaTime);
    }

    // ========================
    // CAMERA LOOK MECHANIC
    // ========================
    private void HandleCameraLook()
    {
        // 1. Horizontal Rotation (Player Body)
        // This makes the entire player capsule turn left/right
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity);

        // 2. Vertical Rotation (Camera only)
        // This handles looking up and down without turning the player capsule
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    // ========================
    // MOVEMENT MECHANIC (Walk/Run)
    // ========================
    private void HandleMovementInput()
    {
        // Check for Running input (Left Shift by default)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Get Input (WASD or Arrow Keys)
        float inputX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float inputZ = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow

        // If the player is grounded, calculate new horizontal movement
        if (characterController.isGrounded)
        {
            // 
            // Calculate direction relative to the player's rotation
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            // Calculate horizontal movement vector
            Vector3 targetMove = (forward * inputZ) + (right * inputX);
            targetMove.Normalize(); // Ensure diagonal movement isn't faster

            // Retain existing vertical motion (gravity/jump)
            float existingY = moveDirection.y;

            // Apply speed to the horizontal movement
            moveDirection = targetMove * currentSpeed;

            // Reapply existing vertical motion
            moveDirection.y = existingY;
        }

        // Apply gravity continuously
        moveDirection.y -= gravity * Time.deltaTime;
    }

    // ========================
    // JUMP MECHANIC
    // ========================
    private void HandleJump()
    {
        // Check if the player is grounded and the Jump key (Spacebar) is pressed
        if (characterController.isGrounded && Input.GetButtonDown("Jump"))
        {
            // v = sqrt(2 * h * g) -> Standard physics formula to calculate upward velocity needed for a given jump height
            moveDirection.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }
    }

    // ========================
    // SHOOTING MECHANIC
    // ========================
 
}
