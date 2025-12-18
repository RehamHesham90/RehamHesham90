using UnityEngine;

/// <summary>
/// PlayerLook: Handles camera rotation (pitch and yaw) using mouse input.
/// This script manages the player's view direction, completely separate from movement.
/// </summary>
public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    // CRITICAL: The empty GameObject that is a child of the Player Root and holds the camera.
    // This is the object that receives vertical rotation (Pitch).
    public Transform cameraHolder;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;

    // Limits the vertical look angle to prevent the camera from flipping over.
    public float verticalLookLimit = 90f;

    // Internal State
    private float xRotation = 0f; // Stores the vertical rotation angle (Pitch)

    void Start()
    {
        // Lock the cursor to the center of the screen and hide it for an immersive FPS experience.
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraHolder == null)
        {
            Debug.LogError("PlayerLook script is missing the Camera Holder reference! Please assign the vertical pivot (Camera Holder).");
            enabled = false;
        }
    }

    void Update()
    {
        // 1. INPUT
        // Get mouse movement input. Use "Mouse X" and "Mouse Y" for raw mouse delta.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 2. VERTICAL ROTATION (Pitch - Up/Down Look)
        // This rotation only affects the camera holder, leaving the player body level.
        xRotation -= mouseY;
        // Clamp the rotation to the vertical limits (-90 to +90 degrees).
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);

        // Apply the vertical rotation to the Camera Holder (the pitch pivot).
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. HORIZONTAL ROTATION (Yaw - Left/Right Look)
        // This rotation affects the entire Player Root object, which carries the Camera Holder.
        transform.Rotate(Vector3.up * mouseX);
    }


}