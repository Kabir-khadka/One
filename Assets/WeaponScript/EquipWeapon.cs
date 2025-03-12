using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EquipWeapon : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField][Range(0.0f, 2.0f)] private float rayLength;
    [SerializeField] private Vector3 rayOffset;
    [SerializeField] private LayerMask weaponMask;
    private RaycastHit topRayHitInfo;
    private RaycastHit bottomRayHitInfo;

    private Weapon currentWeapon;

    [Header("Aiming Positions")]
    [SerializeField] private Transform pistolAimingPos;  // NEW
    [SerializeField] private Transform rifleAimingPos;   // NEW

    [Header("Equip Positions")]
    [SerializeField] private Transform pistolEquipPos;
    [SerializeField] private Transform rifleEquipPos;

    private Animator playerAnimator;

    private bool isAiming = false;

    [Header("Right Hand Target")]
    [SerializeField] private TwoBoneIKConstraint rightHandIK;
    [SerializeField] private Transform rightHandTarget;

    [Header("Left Hand Target")]
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandTarget;

    [SerializeField] private Transform IKRightHandPos;
    [SerializeField] private Transform IKLeftHandPos;

    private bool isEquiped;

    [Header("AK47 Hand Positions")]
    [SerializeField] private Transform AKRightHandPos;
    [SerializeField] private Transform AKLeftHandPos;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Equip();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            UnEquip();
        }

        // Toggle Aim (Fire2 - Right Click)
        if (Input.GetButtonDown("Fire2"))
        {
            isAiming = !isAiming; // Toggle aiming state

            if (currentWeapon)
            {
                if(currentWeapon.WeaponType == WeaponType.Pistol)
                {
                    playerAnimator.SetBool("RevolverAim", isAiming);
                }
                else if(currentWeapon.WeaponType == WeaponType.Rifle)
                {
                    playerAnimator.SetBool("AssaultAim", isAiming);
                }
            }
        }

        // Adjust Weapon Position Based on Aim State
        if (currentWeapon)
        {
            if (isAiming)
            {
                if(currentWeapon.WeaponType == WeaponType.Pistol)
                {
                    // Attach Weapon to Aim Position
                    currentWeapon.transform.SetParent(pistolAimingPos);
                }
                else if (currentWeapon.WeaponType == WeaponType.Rifle)
                {
                    currentWeapon.transform.SetParent(rifleAimingPos);
                }


                currentWeapon.transform.localPosition = Vector3.zero;
                currentWeapon.transform.localRotation = Quaternion.identity;

                //Enable IK for both hands
                leftHandIK.weight = 1f;
                rightHandIK.weight = 1f;

                //Adjust IK positions based on weapon type
                if (currentWeapon.WeaponType == WeaponType.Pistol)
                {
                    leftHandTarget.position = IKLeftHandPos.position;
                    leftHandTarget.rotation = IKLeftHandPos.rotation;


                    rightHandTarget.position = IKRightHandPos.position;
                    rightHandTarget.rotation = IKRightHandPos.rotation;
                }
                else if (currentWeapon.WeaponType == WeaponType.Rifle)
                {
                    leftHandTarget.position = AKLeftHandPos.position;
                    leftHandTarget.rotation = AKLeftHandPos.rotation;

                    rightHandTarget.position = AKRightHandPos.position;
                    rightHandTarget.rotation = AKRightHandPos.rotation;
                }

            }
            else
            {
                // Attach to correct equip position
                if (currentWeapon.WeaponType == WeaponType.Pistol)
                {
                    currentWeapon.transform.SetParent(pistolEquipPos);
                }
                else if (currentWeapon.WeaponType == WeaponType.Rifle)
                {
                    currentWeapon.transform.SetParent(rifleEquipPos);
                }

                currentWeapon.transform.localPosition = Vector3.zero;
                currentWeapon.transform.localRotation = Quaternion.identity;

                //Diable IK when not aiming
                leftHandIK.weight = 0f;
                rightHandIK.weight = 0f;
            }
        }

        // Smoothly Adjust Right-Hand IK When Running
        bool isRunning = playerAnimator.GetBool("IsRunning");

        if (currentWeapon)
        {
            if (currentWeapon.WeaponType == WeaponType.Pistol)
            {
                rightHandIK.weight = isAiming ? (isRunning ? 0.5f : 1f) : 0f;
            }

            else if (currentWeapon.WeaponType == WeaponType.Rifle)
            {
                rightHandIK.weight = isAiming ? (isRunning ? 0.3f : 0.8f) : 0f;
                leftHandIK.weight = isAiming ? (isRunning ? 0.3f : 0.8f) : 0f;
            }
        }
        rightHandIK.weight = isAiming ? (isRunning ? 0.5f : 1f) : 0f;
    }


    private void RaycastsHandler()
    {
        Ray topRay = new Ray(transform.position + rayOffset, transform.forward);
        Ray bottomRay = new Ray(transform.position + Vector3.up * 0.375f, transform.forward);

        Debug.DrawRay(transform.position + rayOffset, transform.forward * rayLength, Color.red);
        Debug.DrawRay(transform.position + Vector3.up * 0.375f, transform.forward * rayLength, Color.green);

        Physics.Raycast(topRay, out topRayHitInfo, rayLength, weaponMask);
        Physics.Raycast(bottomRay, out bottomRayHitInfo, rayLength, weaponMask);
    }

    private void Equip()
    {
        RaycastsHandler();

        if (topRayHitInfo.collider != null)
        {
            currentWeapon = topRayHitInfo.transform.gameObject.GetComponent<Weapon>();
        }

        if (bottomRayHitInfo.collider)
        {
            currentWeapon = bottomRayHitInfo.transform.gameObject.GetComponent<Weapon>();
        }

        if (!currentWeapon) return;

        // Stop weapon rotation when equipped
        currentWeapon.isRotating = false;

        currentWeapon.ChangeBehaviour();

        isEquiped = true;

        // Set correct equip position
        if (currentWeapon.WeaponType == WeaponType.Pistol)
        {
            currentWeapon.transform.SetParent(pistolEquipPos);
        }
        else if (currentWeapon.WeaponType == WeaponType.Rifle)
        {
            currentWeapon.transform.SetParent(rifleEquipPos);
        }

        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }


    private void UnEquip()
    {
        if (isEquiped)
        {
            rightHandIK.weight = 0.0f;

            if (IKLeftHandPos)
            {
                leftHandIK.weight = 0.0f;
            }

            isEquiped = false;
            currentWeapon.transform.parent = null;

            currentWeapon.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            currentWeapon.GetComponent<Rigidbody>().isKinematic = false;

            currentWeapon.isRotating = true;  //This ensures it rotates after being dropped

            currentWeapon = null;
        }
    }
}