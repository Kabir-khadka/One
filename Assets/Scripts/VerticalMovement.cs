using UnityEngine;

public class VerticalMovement : ObstacleBase
{
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;
    [SerializeField] private float moveSpeed = 3f;

    private Vector3 targetPosition;
    private bool isMoving = false; // Only move when player is on it

    private void Start()
    {
        targetPosition = topPoint.position; // Start by moving up
    }

    private void Update()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Check if we've reached the target and switch direction
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = (targetPosition == topPoint.position) ? bottomPoint.position : topPoint.position;
        }
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return 0f; // No knockback needed for a platform
    }

    protected override void ApplyKnockback(GameObject player, float force)
    {
        // No knockback effect needed
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform); // Attach player to platform
            isMoving = true;// Start moving when player steps on
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null); // Detach player from platform
            isMoving = false; // Stop moving when player steps off
        }
    }
}
