using UnityEngine;

public class MovingPlatform : ObstacleBase
{

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 3f;

    private Vector3 targetPosition;

    private bool isMoving = false;// Only move when player is on it.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPosition = pointB.position; //Start by moving toward PointB
    }

    // Update is called once per frame
    void Update()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        //Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed);

        // Check if we.ve reached the target and switch to the other point
        if(Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = (targetPosition == pointA.position) ? pointB.position : pointA.position;
        }
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return 0f; //No knockback needed for this platform
    }

    protected override void ApplyKnockback(GameObject player, float force)
    {
        // No knockback effect needed
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform); //Attach player to platform
            isMoving = true; // Start moving when player steps on
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null); //Detach player from platform
            isMoving = false; // Stop moving when player steps off
        }
    }
}
