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

    public Weapon currentWeapon;
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

    [Header("Weapon Equip Positions")]
    [SerializeField] private Transform pistolEquipPos;
    [SerializeField] private Transform rifleEquipPos;
    [SerializeField] private Transform iceGunEquipPos;
    [SerializeField] private Transform electricGunEquipPos;
    [SerializeField] private Transform bubbleGunEquipPos;

    [Header("Weapon Aim Positions")]
    [SerializeField] private Transform pistolAimingPos;
    [SerializeField] private Transform rifleAimingPos;
    [SerializeField] private Transform iceGunAimingPos;
    [SerializeField] private Transform electricGunAimPos;
    [SerializeField] private Transform bubbleGunAimPos;

    private Dictionary<WeaponType, Transform> equipPositions = new Dictionary<WeaponType, Transform>();
    private Dictionary<WeaponType, Transform> aimPositions = new Dictionary<WeaponType, Transform>();

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        aimTarget = GameObject.Find("AimTarget")?.transform;
        if (aimTarget == null)
        {
            Debug.LogWarning("AimTarget not found in scene. Please create an empty GameObject named 'AimTarget'.");
        }

        equipPositions[WeaponType.Pistol] = pistolEquipPos;
        equipPositions[WeaponType.Rifle] = rifleEquipPos;
        equipPositions[WeaponType.Ice_Gun] = iceGunEquipPos;
        equipPositions[WeaponType.Electric_Gun] = electricGunEquipPos;
        equipPositions[WeaponType.Bubble_Gun] = bubbleGunEquipPos;

        aimPositions[WeaponType.Pistol] = pistolAimingPos;
        aimPositions[WeaponType.Rifle] = rifleAimingPos;
        aimPositions[WeaponType.Ice_Gun] = iceGunAimingPos;
        aimPositions[WeaponType.Electric_Gun] = electricGunAimPos;
        aimPositions[WeaponType.Bubble_Gun] = bubbleGunAimPos;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Equip();
        if (Input.GetKeyDown(KeyCode.Q)) UnEquip();
        if (Input.GetButtonDown("Fire2") && currentWeapon != null)
        {
            isAiming = !isAiming;
            aimCam.SetActive(isAiming);
            playerAnimator.SetBool("IsAiming", isAiming);
        }

        if (currentWeapon)
        {
            Transform targetPos = isAiming ? aimPositions[currentWeapon.WeaponType] : equipPositions[currentWeapon.WeaponType];
            currentWeapon.transform.SetParent(targetPos);
            currentWeapon.transform.position = targetPos.position;
            currentWeapon.transform.rotation = targetPos.rotation;


            if (isAiming)
            {
                leftHandIK.weight = 1f;
                rightHandIK.weight = 1f;

                SetDynamicHandPositions();
            }
            else
            {
                leftHandIK.weight = 0f;
                rightHandIK.weight = 0f;
            }
        }
    }

    private void SetDynamicHandPositions()
    {
        Transform rightHandPos = null, leftHandPos = null;

        switch (currentWeapon.WeaponType)
        {
            case WeaponType.Pistol:
                rightHandPos = currentWeapon.transform.Find("NPistolRightHandPos") ?? currentWeapon.transform.Find("HeavyPistolRightHandPos");
                leftHandPos = currentWeapon.transform.Find("NPistolLeftHandPos") ?? currentWeapon.transform.Find("HeavyPistolLeftHandPos");
                break;
            case WeaponType.Rifle:
                rightHandPos = currentWeapon.transform.Find("AKRightHandPos");
                leftHandPos = currentWeapon.transform.Find("AKLeftHandPos");
                break;
            case WeaponType.Ice_Gun:
                rightHandPos = currentWeapon.transform.Find("IceGunRightHandPos");
                leftHandPos = currentWeapon.transform.Find("IceGunLeftHandPos");
                break;

            case WeaponType.Electric_Gun:
                rightHandPos = currentWeapon.transform.Find("ElecRightHandPos");
                leftHandPos = currentWeapon.transform.Find("ElecLeftHandPos");
                break;

            case WeaponType.Bubble_Gun:
                rightHandPos = currentWeapon.transform.Find("BubRightHandPos");
                leftHandPos = currentWeapon.transform.Find("BubLeftHandPos");
                break;
        }

        if (rightHandPos)
        {
            rightHandTarget.position = rightHandPos.position;
            rightHandTarget.rotation = rightHandPos.rotation;
        }

        if (leftHandPos)
        {
            leftHandTarget.position = leftHandPos.position;
            leftHandTarget.rotation = leftHandPos.rotation;
        }
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
        if (topRayHitInfo.collider != null) currentWeapon = topRayHitInfo.transform.GetComponent<Weapon>();
        if (bottomRayHitInfo.collider) currentWeapon = bottomRayHitInfo.transform.GetComponent<Weapon>();
        if (!currentWeapon) return;

        currentWeapon.isRotating = false;
        currentWeapon.ChangeBehaviour();
        isEquiped = true;

        if (!equipPositions.TryGetValue(currentWeapon.WeaponType, out Transform equipPos)) return;

        currentWeapon.transform.SetParent(equipPos);
        currentWeapon.transform.position = equipPos.position;
        currentWeapon.transform.rotation = equipPos.rotation;
    }

    public bool IsWeaponEquipped()
    {
        return currentWeapon != null; // Returns true if a weapon is equipped
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
