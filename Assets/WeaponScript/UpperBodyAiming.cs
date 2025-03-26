using UnityEngine;
using UnityEngine.Animations.Rigging;

public class UpperBodyAim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Rig upperBodyRig;
    [SerializeField] private LayerMask aimColliderLayerMask;
    [SerializeField] private Transform playerBody; // NEW - Reference to the player's transform

    [Header("Settings")]
    [SerializeField] private float bodyRotationSpeed = 10f; // NEW - Rotation speed

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;

    private ThirdPersonShooterController shooterController;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        shooterController = GetComponent<ThirdPersonShooterController>();

        if (upperBodyRig != null)
        {
            upperBodyRig.weight = 0f;
        }
    }

    void Update()
    {
        if (aimTarget == null || mainCamera == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
        }

        Vector3 targetPosition = ray.origin + ray.direction * 100f;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 999f, aimColliderLayerMask))
        {
            targetPosition = hitInfo.point;

            if (showDebugRay)
            {
                Debug.DrawLine(ray.origin, targetPosition, Color.green);
            }
        }

        aimTarget.position = targetPosition;

        if (upperBodyRig != null)
        {
            bool isAiming = shooterController.isAiming;
            upperBodyRig.weight = Mathf.Lerp(upperBodyRig.weight, isAiming ? 1f : 0f, Time.deltaTime * 10f);
        }

        // NEW - Rotate the player's body towards the aim direction
        if (shooterController.isAiming && playerBody != null)
        {
            Vector3 aimDirection = (aimTarget.position - playerBody.position).normalized;
            aimDirection.y = 0; // Keep rotation on horizontal plane

            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
        }
    }
}
