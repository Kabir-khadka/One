using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ThirdPersonShooterController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] public CinemachineVirtualCamera thirdPersonCam; // Standard third-person camera
    [SerializeField] public CinemachineVirtualCamera aimCam; // Aiming camera
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask(); // Layer mask for aiming collision detection
    [SerializeField] private Transform pfBulletProjectile; // Prefab for bullets
    [SerializeField] private Transform spawnBulletPosition; // Position where bullets are spawned
    [SerializeField] private ParticleSystem iceParticleSystem; // Refernce to Ice_Gun's particle effect

    public GameObject CinemachineCameraTarget; // Camera target for rotation
    public float TopClamp = 70.0f; // Upper limit for vertical camera rotation
    public float BottomClamp = -30.0f; // Lower limit for vertical camera rotation
    public float CameraAngleOverride = 0.0f; // Additional angle offset
    public float Sensitivity = 1.0f; // Mouse sensitivity

    private EquipWeapon equipWeapon; // Reference to the weapon equip script
    public bool isAiming = false; // Aiming state

    private float _cinemachineTargetYaw; // Yaw rotation value
    private float _cinemachineTargetPitch; // Pitch rotation value

    private Vector3 mouseWorldPosition = Vector3.zero; // World position of the aiming point

    [SerializeField] private float normalSensitivity; // Sensitivity for normal mode
    [SerializeField] private float aimSensitivity; // Sensitivity for aiming mode
    
    private AnimandMovement animAndMovement; // Reference to movement and animation script
    private Transform aimTarget; // Target the player aims at

    [SerializeField] private float nextFireTime = 0f;

    // Dictionary to store fire rates per weapon type
    private Dictionary<WeaponType, float> fireRates = new Dictionary<WeaponType, float>()
    {
        { WeaponType.Pistol, 0.3f }, // Semi-Auto
        { WeaponType.Rifle, 0.1f }, //Automatic
        { WeaponType.Ice_Gun, 0.07f }, // Automatic (faster)
        { WeaponType.Bubble_Gun, 0.5f }, //Slow-Fire
        { WeaponType.Electric_Gun, 0.8f }, // Charged Shot (Later)
    };

    private void Start()
    {
        animAndMovement = GetComponent<AnimandMovement>();
        equipWeapon = GetComponent<EquipWeapon>();

        // Find the Cinemachine camera target if not assigned
        if (CinemachineCameraTarget == null)
        {
            CinemachineCameraTarget = GameObject.FindGameObjectWithTag("CinemachineTarget");
        }

        // Find or create the AimTarget object
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
        // Get the screen center position for raycasting
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Raycast to detect where the player is aiming
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit rayCastHit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = rayCastHit.point;
        }

        // Update the aim target position
        if (aimTarget != null && mouseWorldPosition != Vector3.zero)
        {
            aimTarget.position = mouseWorldPosition;
        }

        // Toggle aiming when the player presses Fire2 (Right Mouse Button by default)
        if (Input.GetButtonDown("Fire2"))
        {
            ToggleAim();
        }

        // Shoot when aiming and pressing Fire1 (Left Mouse Button by default)
        if (isAiming && equipWeapon != null && equipWeapon.IsWeaponEquipped())
        {
            WeaponType currentWeaponType = equipWeapon.currentWeapon.WeaponType;

            if (currentWeaponType == WeaponType.Ice_Gun)
            {
                // Ice Gun: Enable and disable particle system on Fire1 input
                if (Input.GetButtonDown("Fire1"))
                {
                    // Rotate the particle system to face the aim direction
                    Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
                    iceParticleSystem.transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);

                    // Immediately start particle system when first pressing the button
                    iceParticleSystem.Play();
                }
                else if (Input.GetButton("Fire1"))
                {
                    // Rotate the particle system to face the aim direction
                    Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
                    iceParticleSystem.transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);

                    // Ensure the system is playing while button is held
                    if (!iceParticleSystem.isPlaying)
                    {
                        iceParticleSystem.Play();
                    }
                }
                else if (Input.GetButtonUp("Fire1"))
                {
                    // Stop when button is released
                    iceParticleSystem.Stop();
                }
            }
            else if (fireRates.ContainsKey(currentWeaponType))
            {
                float fireRate = fireRates[currentWeaponType];

                // Automatic Weapons: Fire continously if Fire1 is held
                if (currentWeaponType == WeaponType.Rifle)
                {
                    if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
                    {
                        nextFireTime = Time.time + fireRate;
                        ShootBullet();
                    }
                }

                // Semi Auto / Slow Weapons: Fire only on button press
                else if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + fireRate;
                    ShootBullet();
                }
            }
        }
        // Handle camera rotation based on mouse movement
        CameraRotation();
    }

    // Toggles aiming mode
    public void ToggleAim()
    {
        if (equipWeapon != null && equipWeapon.IsWeaponEquipped())
        {
            isAiming = !isAiming;

            if (isAiming)
            {
                // Switch to aiming camera
                aimCam.Priority = 11;
                thirdPersonCam.Priority = 9;
                aimCam.gameObject.SetActive(true);
                thirdPersonCam.gameObject.SetActive(false);
                SetSensitivity(aimSensitivity);
                animAndMovement.SetRotateOnMove(false);
            }
            else
            {
                // Switch back to third-person camera
                aimCam.Priority = 9;
                thirdPersonCam.Priority = 11;
                aimCam.gameObject.SetActive(false);
                thirdPersonCam.gameObject.SetActive(true);
                SetSensitivity(normalSensitivity);
                animAndMovement.SetRotateOnMove(true);
            }
        }
        else
        {
            isAiming = false;
        }
    }

    // Handles shooting mechanics
    public void ShootBullet()
    {
            // Determine the direction of the bullet
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
    }

    // Handles camera rotation based on mouse movement
    private void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _cinemachineTargetYaw += mouseX * Sensitivity;
        _cinemachineTargetPitch -= mouseY * Sensitivity;

        // Clamp vertical rotation to avoid flipping the camera
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Apply rotation to the camera target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    // Sets the camera sensitivity
    public void SetSensitivity(float newSensitivity)
    {
        Sensitivity = newSensitivity;
    }
}