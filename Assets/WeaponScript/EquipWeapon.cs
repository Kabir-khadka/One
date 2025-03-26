using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

public class EquipWeapon : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField][Range(0.0f, 2.0f)] private float rayLength;
    [SerializeField] private Vector3 rayOffset;
    [SerializeField] private LayerMask weaponMask;
    private RaycastHit topRayHitInfo;
    private RaycastHit bottomRayHitInfo;

    private Weapon currentWeapon;
    private Animator playerAnimator;
    private bool isAiming = false;
    private bool isEquiped;

    [Header("IK Constraints")]
    [SerializeField] private TwoBoneIKConstraint rightHandIK;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandTarget;

    [SerializeField] private GameObject aimCam;
    private Transform aimTarget;

    [Header("Weapon Positions")]
    [SerializeField] private Transform pistolEquipPos;
    [SerializeField] private Transform rifleEquipPos;
    [SerializeField] private Transform pistolAimingPos;
    [SerializeField] private Transform rifleAimingPos;
    [SerializeField] private Transform iceGunEquipPos;
    [SerializeField] private Transform iceGunAimPos;
    

    private Dictionary<WeaponType, Transform> equipPositions = new Dictionary<WeaponType, Transform>();
    private Dictionary<WeaponType, Transform> aimPositions = new Dictionary<WeaponType, Transform>();

    private Transform IKRightHandPos;
    private Transform IKLeftHandPos;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        aimTarget = GameObject.Find("AimTarget")?.transform;
        if (aimTarget == null)
        {
            Debug.LogWarning("AimTarget not found in scene. Please create an empty GameObject named 'AimTarget'.");
        }

        // Initialize equip and aim positions
        equipPositions[WeaponType.Pistol] = pistolEquipPos;
        equipPositions[WeaponType.Rifle] = rifleEquipPos;
        equipPositions[WeaponType.Ice_Gun] = iceGunEquipPos;


       //aiming positions
        aimPositions[WeaponType.Pistol] = pistolAimingPos;
        aimPositions[WeaponType.Rifle] = rifleAimingPos;
        aimPositions[WeaponType.Ice_Gun] = iceGunAimPos;


    }

    private void Update()
    {
        // Check if the player is running
        bool isRunning = playerAnimator.GetBool("isRunning");

        if (Input.GetKeyDown(KeyCode.E)) Equip();
        if (Input.GetKeyDown(KeyCode.Q)) UnEquip();

        if (Input.GetButtonDown("Fire2") && currentWeapon != null)
        {
            isAiming = !isAiming;
            aimCam.SetActive(isAiming);
            playerAnimator.SetBool(currentWeapon.WeaponType == WeaponType.Pistol 
                || currentWeapon.WeaponType == WeaponType.Ice_Gun ? "RevolverAim" : "AssaultAim", isAiming);
        }

        if (currentWeapon)
        {
            // Determine the target position based on aiming
            Transform targetPos = isAiming ? aimPositions[currentWeapon.WeaponType] : equipPositions[currentWeapon.WeaponType];
            currentWeapon.transform.SetParent(targetPos);
            currentWeapon.transform.position = targetPos.position;
            currentWeapon.transform.rotation = targetPos.rotation;

            // Adjust the weapon's rotation while aiming
            if (isAiming && aimTarget != null)
            {
                Vector3 aimDir = (aimTarget.position - currentWeapon.transform.position).normalized;
                targetPos.rotation = Quaternion.LookRotation(aimDir);
            }

            // Adjust IK weights based on aiming and running states
            if (isAiming)
            {
                leftHandIK.weight = isRunning ? 0.3f : 1f;
                rightHandIK.weight = isRunning ? 0.3f : 1f;

                // Adjust right-hand IK based on running state
                if (currentWeapon.WeaponType == WeaponType.Pistol)
                {
                    rightHandIK.weight = isRunning ? 0.5f : 1f;
                }
                else if (currentWeapon.WeaponType == WeaponType.Rifle)
                {
                    rightHandIK.weight = isRunning ? 0.3f : 0.8f;
                    leftHandIK.weight = isRunning ? 0.3f : 0.8f;
                }
                else if (currentWeapon.WeaponType == WeaponType.Ice_Gun)
                {
                    rightHandIK.weight = isRunning ? 0.3f : 0.8f;
                    leftHandIK.weight = isRunning ? 0.3f : 0.8f;
                }

                // Find hand positions for IK targeting
                IKRightHandPos = currentWeapon.transform.Find("RightHandPos");
                IKLeftHandPos = currentWeapon.transform.Find("LeftHandPos");

                // Set right hand IK target position and rotation
                if (IKRightHandPos != null)
                {
                    rightHandTarget.position = IKRightHandPos.position;
                    rightHandTarget.rotation = IKRightHandPos.rotation;
                }
                else
                {
                    Debug.LogWarning("Right Hand Pos not found in currentWeapon!");
                }

                // Set left hand IK target position and rotation
                if (IKLeftHandPos != null)
                {
                    leftHandTarget.position = IKLeftHandPos.position;
                    leftHandTarget.rotation = IKLeftHandPos.rotation;
                }
                else
                {
                    Debug.LogWarning("Left Hand Pos not found in currentWeapon!");
                }
            }
            else
            {
                // Reset IK weights when not aiming
                leftHandIK.weight = 0f;
                rightHandIK.weight = 0f;
            }
        }
    }


    private void RaycastsHandler()
    {
        Ray topRay = new Ray(transform.position + rayOffset, transform.forward);
        Ray bottomRay = new Ray(transform.position + Vector3.up * 0.375f, transform.forward);
        Debug.DrawRay(topRay.origin, topRay.direction * rayLength, Color.red);
        Debug.DrawRay(bottomRay.origin, bottomRay.direction * rayLength, Color.green);

        Physics.Raycast(topRay, out topRayHitInfo, rayLength, weaponMask);
        Physics.Raycast(bottomRay, out bottomRayHitInfo, rayLength, weaponMask);
    }

    private void Equip()
    {
        RaycastsHandler();

        if (topRayHitInfo.collider != null)
        {
            currentWeapon = topRayHitInfo.transform.GetComponent<Weapon>();
        }
        if (bottomRayHitInfo.collider)
        {
            currentWeapon = bottomRayHitInfo.transform.GetComponent<Weapon>();
        }
        if (!currentWeapon) return;

        currentWeapon.isRotating = false;
        currentWeapon.ChangeBehaviour();
        isEquiped = true;

        if (!equipPositions.TryGetValue(currentWeapon.WeaponType, out Transform equipPos))
        {
            Debug.LogError("No equip position defined for " + currentWeapon.WeaponType);
            return;
        }

        currentWeapon.transform.SetParent(equipPos);
        currentWeapon.transform.position = equipPos.position;
        currentWeapon.transform.rotation = equipPos.rotation;
    }

    public bool IsWeaponEquipped()
    {
        return currentWeapon != null;
    }

    private void UnEquip()
    {
        if (isEquiped)
        {
            rightHandIK.weight = 0.0f;
            leftHandIK.weight = 0.0f;
            isEquiped = false;
            currentWeapon.transform.parent = null;
            currentWeapon.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            currentWeapon.GetComponent<Rigidbody>().isKinematic = false;
            currentWeapon.isRotating = true;
            currentWeapon = null;
        }
    }
}
