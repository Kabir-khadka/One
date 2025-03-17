using UnityEngine;
using Cinemachine;

public class CinemachineAiming : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineFreeLook thirdPersonCam;
    [SerializeField] private CinemachineVirtualCamera aimCam;

    [Header("Zoom Settings")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomedFOV = 40f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Aiming Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerBody;
    private bool isAiming = false;

    [Header("Rotation Settings")]
    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 30f;

    [Header("Weapon Settings")]
    private Transform weaponTransform;
    [SerializeField] private float weaponRotationSpeed = 15f;

    // Reference to aim camera components
    private Transform aimCamTransform;

    void Start()
    {
        playerBody = transform;

        // Cache the aim camera transform for efficiency
        if (aimCam != null)
        {
            aimCamTransform = aimCam.transform;
        }
    }

    void LateUpdate()
    {
        HandleAimToggle();
        UpdateFOV();

        if (isAiming)
        {
            HandleRotation();
        }
    }

    private void HandleAimToggle()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            isAiming = !isAiming;
            aimCam.Priority = isAiming ? 11 : 9;

            // Reset rotations when entering aim mode
            if (isAiming)
            {
                // Store current camera rotation as starting point
                if (Camera.main != null)
                {
                    horizontalRotation = Camera.main.transform.eulerAngles.y;
                    verticalRotation = 0f; // Reset vertical rotation
                }
            }
        }
    }

    private void UpdateFOV()
    {
        float targetFOV = isAiming ? zoomedFOV : normalFOV;
        thirdPersonCam.m_Lens.FieldOfView = Mathf.Lerp(thirdPersonCam.m_Lens.FieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
        aimCam.m_Lens.FieldOfView = Mathf.Lerp(aimCam.m_Lens.FieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Update rotation values
        horizontalRotation += mouseX;
        verticalRotation -= mouseY; // Inverted because mouse up should rotate camera down
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        // Rotate player body horizontally
        if (playerBody != null)
        {
            // Only update the Y rotation of the player
            Quaternion targetRotation = Quaternion.Euler(0, horizontalRotation, 0);
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRotation, 10 * Time.deltaTime);
        }

        // Directly set the aim camera rotation
        if (aimCamTransform != null)
        {
            // Set the proper rotation for the aim camera (both horizontal and vertical)
            aimCamTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        // Rotate weapon to match aim direction
        if (weaponTransform != null)
        {
            // Calculate the target rotation
            Quaternion targetWeaponRotation = Quaternion.Euler(verticalRotation, 0, 0);

            // Apply to weapon's local rotation (assuming weapon is child of player)
            weaponTransform.localRotation = Quaternion.Slerp(
                weaponTransform.localRotation,
                targetWeaponRotation,
                weaponRotationSpeed * Time.deltaTime
            );
        }
    }

    public void SetWeaponTransform(Transform newWeaponTransform)
    {
        weaponTransform = newWeaponTransform;
    }
}