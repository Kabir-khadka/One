using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class ThirdPersonShooterController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] public CinemachineVirtualCamera thirdPersonCam;
    [SerializeField] public CinemachineVirtualCamera aimCam;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform spawnBulletPosition;

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

    private Vector3 mouseWorldPosition = Vector3.zero;

    //Sensitivity variables for both cameras.
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;

    private AnimandMovement animAndMovement;

    // Add a reference to AimTarget
    private Transform aimTarget;

    private void Start()
    {
        animAndMovement = GetComponent<AnimandMovement>();
        equipWeapon = GetComponent<EquipWeapon>(); // Assuming it's on the same GameObject

        // Ensure CinemachineCameraTarget is assigned in the Inspector
        if (CinemachineCameraTarget == null)
        {
            CinemachineCameraTarget = GameObject.FindGameObjectWithTag("CinemachineTarget"); // Find by tag
        }

        // Find the AimTarget in the scene
        aimTarget = GameObject.Find("AimTarget")?.transform;
        if (aimTarget == null)
        {
            Debug.LogWarning("AimTarget not found. Creating one.");
            GameObject newAimTarget = new GameObject("AimTarget");
            aimTarget = newAimTarget.transform;
        }
    }

    void Update()
    {
        //Getting center of the screen since it never changes and also
        //it helps the aim not to break even if there is no mouse connected
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit rayCastHit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = rayCastHit.point;
        }

        // Update AimTarget position based on raycast hit
        if (aimTarget != null && mouseWorldPosition != Vector3.zero)
        {
            aimTarget.position = mouseWorldPosition;
        }

        // Toggle aim mode with the "Fire2" button (usually right-click or another input)
        if (Input.GetButtonDown("Fire2"))
        {
            ToggleAim();
        }

        if (isAiming)
        {
            // Optional: You can leave RotatePlayerTowardsMouse() for full body rotation
            // or comment it out if you only want upper body rotation
            // RotatePlayerTowardsMouse();
        }

        //Shooting logic (check if Fire1 is pressed while aiming
        if (isAiming && Input.GetButtonDown("Fire1"))
        {
            ShootBullet();
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
                SetSensitivity(aimSensitivity);
                animAndMovement.SetRotateOnMove(false);
            }
            else
            {
                aimCam.Priority = 9;
                thirdPersonCam.Priority = 11;
                aimCam.gameObject.SetActive(false); // Disable aim camera
                thirdPersonCam.gameObject.SetActive(true); // Enable third person camera
                SetSensitivity(normalSensitivity);
                animAndMovement.SetRotateOnMove(true);
            }
        }
        else
        {
            isAiming = false; // Prevent aiming if no weapon is equipped
        }
    }

    public void ShootBullet()
    {
        Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
        Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
    }

    private void RotatePlayerTowardsMouse()
    {
        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
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

    public void SetSensitivity(float newSensitivity)
    {
        Sensitivity = newSensitivity;
    }
}