using UnityEngine;
using Cinemachine;           
using UnityEngine.InputSystem; 
using System.Collections.Generic; 


public class ThirdPersonShooterController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] public CinemachineVirtualCamera thirdPersonCam;  // Camera for normal third-person view
    [SerializeField] public CinemachineVirtualCamera aimCam;          // Camera for aiming view (zoomed in)
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();  // Layers that can be targeted when aiming
    [SerializeField] private Transform spawnBulletPosition;           // Position where bullets will spawn from
    [SerializeField] private ParticleSystem iceParticleSystem;        // Particle system for ice weapon effects

    [Header("Ice Gun Time")]
    private float iceGunTimeUsed = 0f;
    [SerializeField] private float iceGunMaxTime = 8f;
    private bool iceGunExpired = false;

    // Camera target and constraints
    public GameObject CinemachineCameraTarget;                       
    public float TopClamp = 70.0f;                                  
    public float BottomClamp = -30.0f;                              
    public float CameraAngleOverride = 0.0f;                         
    public float Sensitivity = 1.0f;                                 

    private EquipWeapon equipWeapon;
    private Weapon currentWeapon;
    public bool isAiming = false;                                    

    // Camera rotation variables
    private float _cinemachineTargetYaw;                              
    private float _cinemachineTargetPitch;                            

    private Vector3 mouseWorldPosition = Vector3.zero;                

    // Sensitivity settings for different states
    [SerializeField] private float normalSensitivity;                
    [SerializeField] private float aimSensitivity;                   

    private AnimandMovement animAndMovement;                          
    private Transform aimTarget;                                     

    // Weapon firing control
    [SerializeField] private float nextFireTime = 0f;                 
    [SerializeField] private GameObject electricBulletPrefab;
    [SerializeField] private GameObject bombBulletPrefab;

    // Dictionary mapping weapon types to their fire rates (cooldown between shots)
    private Dictionary<WeaponType, float> fireRates = new Dictionary<WeaponType, float>()
    {
        { WeaponType.Pistol, 0.3f },                                  
        { WeaponType.Rifle, 0.1f },                                   
        { WeaponType.Ice_Gun, 0.07f },                                
        { WeaponType.Bubble_Gun, 0.5f },                              
        { WeaponType.Electric_Gun, 0.8f },                            // Electric gun can fire every 0.8 seconds
    };

    
    private void Start()
    {
        // Get references to required components on the same GameObject
        animAndMovement = GetComponent<AnimandMovement>();
        equipWeapon = GetComponent<EquipWeapon>();

        // Find camera target if not assigned in inspector
        if (CinemachineCameraTarget == null)
        {
            CinemachineCameraTarget = GameObject.FindGameObjectWithTag("CinemachineTarget");
        }

        // Find or create the aim target object
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

        // Add this near the beginning of Update
        if (isAiming && (equipWeapon == null || !equipWeapon.IsWeaponEquipped()))
        {
            ExitAimMode();
        }

        // Cast a ray from screen center to determine where player is aiming in the world
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit rayCastHit, 999f, aimColliderLayerMask))
        {
            mouseWorldPosition = rayCastHit.point;
        }

        // Update aim target position to where player is aiming
        if (aimTarget != null && mouseWorldPosition != Vector3.zero)
        {
            aimTarget.position = mouseWorldPosition;
        }

        // Toggle aiming with right mouse button (Fire2)
        if (Input.GetButtonDown("Fire2"))
        {
            ToggleAim();
        }

        // Handle weapon firing when aiming and weapon is equipped
        if (isAiming && equipWeapon != null && equipWeapon.IsWeaponEquipped())
        {
            currentWeapon = equipWeapon.currentWeapon;
            WeaponType currentWeaponType = equipWeapon.currentWeapon.WeaponType;

            // Handle Ice Gun - continuous particle effect while firing
            if (currentWeaponType == WeaponType.Ice_Gun)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    // Start ice particle effect when mouse button first pressed
                    Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
                    iceParticleSystem.transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);
                    iceParticleSystem.Play();
                }
                else if (Input.GetButton("Fire1"))
                {
                    // Keep updating ice particle direction while button held
                    Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
                    iceParticleSystem.transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);
                    if (!iceParticleSystem.isPlaying)
                        iceParticleSystem.Play();

                    // Count only when firing
                    iceGunTimeUsed += Time.deltaTime;

                    if (iceGunTimeUsed >= iceGunMaxTime)
                    {
                        iceGunExpired = true;
                        iceParticleSystem.Stop();
                        ExitAimMode(); // Call this before destroying the weapon.
                        Destroy(currentWeapon.gameObject);
                        equipWeapon.UnEquip();
                        Debug.Log("Ice Gun destroyed after 8 seconds of use");
                    }
                    
                }
                else if (Input.GetButtonUp("Fire1"))
                {
                    // Stop ice particle effect when button released
                    iceParticleSystem.Stop();
                }
            }
            // Limited-shot weapons like Bubble_Gun and Electric_Gun
            else if (currentWeapon.UsesLimitedBullets())
            {
                HandleLimitedShotWeapon(currentWeapon);
            }
        }


        // Update camera rotation based on mouse input
        CameraRotation();
    }

    //Method to exit out of aim mode when Weapon is destroyed.
    private void ExitAimMode()
    {
        if (isAiming)
        {
            isAiming = false;
            aimCam.Priority = 9;
            thirdPersonCam.Priority = 11;
            aimCam.gameObject.SetActive(false);
            thirdPersonCam.gameObject.SetActive(true);
            SetSensitivity(normalSensitivity);
            animAndMovement.SetRotateOnMove(true);
            animAndMovement.SetAimingState(false);
        }
    }
    public void ToggleAim()
    {
        if (equipWeapon != null && equipWeapon.IsWeaponEquipped())
        {
            isAiming = !isAiming;

            if (isAiming)
            {
                // Switch to aim camera and adjust settings for aiming
                aimCam.Priority = 11;                 // Higher priority makes this camera active
                thirdPersonCam.Priority = 9;
                aimCam.gameObject.SetActive(true);
                thirdPersonCam.gameObject.SetActive(false);
                SetSensitivity(aimSensitivity);       // Reduce sensitivity for precision aiming
                //animAndMovement.SetRotateOnMove(false); // Prevent character rotation during aim mode
                animAndMovement.SetAimingState(true); // Add this line to force walk mode
            }
            else
            {
                // Switch back to third-person camera and normal settings
                aimCam.Priority = 9;
                thirdPersonCam.Priority = 11;
                aimCam.gameObject.SetActive(false);
                thirdPersonCam.gameObject.SetActive(true);
                SetSensitivity(normalSensitivity);    // Return to normal sensitivity
                //animAndMovement.SetRotateOnMove(true); // Allow character rotation during movement again
                animAndMovement.SetAimingState(false); // Add this line to allow running again
            }
        }
        else
        {
            //Force exit aim mode if no weapon is equipped
            ExitAimMode();
            // Can't aim without a weapon
            isAiming = false;
        }
    }

    private void HandleLimitedShotWeapon(Weapon weapon)
    {
        WeaponType weaponType = weapon.WeaponType;

        if(Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            if (!weapon.IsOutOfBullets())
            {
                nextFireTime = Time.time + fireRates[weaponType];

                // Fire correct bullet type based on weapon
                switch (weaponType)
                {
                    case WeaponType.Bubble_Gun:
                        ShootBombBullet();
                        break;
                    case WeaponType.Electric_Gun:
                        ShootElectricBullet();
                        break;
                    //Add more limited-shot weapons here if needed
                }

                // Decrease bullet count
                weapon.DecreaseBullet();

                // If bullets are finished, destroy and unequip
                if (weapon.IsOutOfBullets())
                {
                    ExitAimMode(); //Call this before destroying the weapon.
                    Destroy(weapon.gameObject);
                    equipWeapon.UnEquip();
                    Debug.Log($"{weaponType} destroyed after max shots.");
                }
            }
        }
    }


    public void ShootElectricBullet()
    {
        if (electricBulletPrefab != null && spawnBulletPosition != null)
        {
            // Calculate the aim direction vector
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;

            // Instantiate the bullet at the spawn position with correct rotation
            GameObject bullet = Instantiate(electricBulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir));

            // Optional: Add a small initial offset to prevent collision with the player
            bullet.transform.position += aimDir * 0.5f;

            // Log for debugging
            Debug.Log($"Electric bullet fired toward: {mouseWorldPosition}, direction: {aimDir}");
        }
        else
        {
            Debug.LogWarning("Cannot shoot electric bullet: missing prefab or spawn position");
        }
    }

    public void ShootBombBullet()
    {
        if(bombBulletPrefab != null && spawnBulletPosition != null)
        {
            // Calculate the aim direction vector
            Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;

            // Instantiate the bullet at the spawn position with correct rotation
            GameObject bullet = Instantiate(bombBulletPrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDir));

            // Optional: Add a small initial offset to prevent collision with the player
            bullet.transform.position += aimDir * 0.5f;

            // Log for debugging
            Debug.Log($"Bomb bullet fired toward: {mouseWorldPosition}, direction: {aimDir}");
        }
        else
        {
            Debug.LogWarning("Cannot shoot Bomb bullet: missing prefab or spawn position");
        }
    }


    private void CameraRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Update camera rotation values based on mouse movement and sensitivity
        _cinemachineTargetYaw += mouseX * Sensitivity;    // Horizontal rotation
        _cinemachineTargetPitch -= mouseY * Sensitivity;  // Vertical rotation (inverted)

        // Clamp vertical rotation to prevent over-rotation
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Apply rotation to camera target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride, // Pitch (looking up/down)
            _cinemachineTargetYaw,                         // Yaw (looking left/right)
            0.0f                                           
        );
    }

    
    public void SetSensitivity(float newSensitivity)
    {
        Sensitivity = newSensitivity;
    }
}