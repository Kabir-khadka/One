using UnityEngine;
using Cinemachine;

public class ThirdPersonShooterController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] public CinemachineVirtualCamera thirdPersonCam;
    [SerializeField] public CinemachineVirtualCamera aimCam;

    // Additional variables for controlling camera rotation
    public GameObject CinemachineCameraTarget; // Target for the camera to follow
    public float TopClamp = 70.0f; // Vertical limit for camera rotation
    public float BottomClamp = -30.0f; // Vertical limit for camera rotation
    public float CameraAngleOverride = 0.0f; // Optional for fine-tuning camera position
    public float Sensitivity = 1.0f; // Mouse sensitivity for camera rotation

    private EquipWeapon equipWeapon;
    public bool isAiming = false;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private void Start()
    {
        equipWeapon = GetComponent<EquipWeapon>(); // Assuming it's on the same GameObject

        // Ensure CinemachineCameraTarget is assigned in the Inspector
        if (CinemachineCameraTarget == null)
        {
            CinemachineCameraTarget = GameObject.FindGameObjectWithTag("CinemachineTarget"); // Find by tag
        }
    }

    void Update()
    {
        // Toggle aim mode with the "Fire2" button (usually right-click or another input)
        if (Input.GetButtonDown("Fire2"))
        {
            ToggleAim();
        }

            CameraRotation(); // Call camera rotation 
        
    }

    public void ToggleAim()
    {
        // Check if a weapon is equipped before allowing aiming
        if (equipWeapon != null && equipWeapon.IsWeaponEquipped())
        {
            isAiming = !isAiming;

            // Adjust camera priority and ensure active camera switch
            if (isAiming)
            {
                aimCam.Priority = 11;  // Higher priority -> becomes active
                thirdPersonCam.Priority = 9; // Lower priority -> deactivates
                aimCam.gameObject.SetActive(true); // Enable aim camera
                thirdPersonCam.gameObject.SetActive(false); // Disable third person camera
            }
            else
            {
                aimCam.Priority = 9;
                thirdPersonCam.Priority = 11;
                aimCam.gameObject.SetActive(false); // Disable aim camera
                thirdPersonCam.gameObject.SetActive(true); // Enable third person camera
            }
        }
        else
        {
            isAiming = false; // Prevent aiming if no weapon is equipped
        }
    }

    // Method to rotate the camera based on mouse input
    private void CameraRotation()
    {
        // Get mouse input and apply sensitivity
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Update yaw (horizontal rotation) and pitch (vertical rotation) based on mouse input
        _cinemachineTargetYaw += mouseX * Sensitivity;
        _cinemachineTargetPitch -= mouseY * Sensitivity;

        // Clamp the pitch to prevent excessive up/down rotation
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Apply the calculated rotation to the CinemachineCameraTarget
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }
}
