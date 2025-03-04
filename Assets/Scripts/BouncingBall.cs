using UnityEngine;

public class BouncingBall : MonoBehaviour
{
    [SerializeField] private float respawnHeight = 5f;
    [SerializeField] private float minYPosition = -5f; // Lower limit before respawning
    [SerializeField] private float knockbackForce = 10f; // Force applied to the player

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (transform.position.y < minYPosition) // If the ball falls too low, respawn it
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        Vector3 newPosition = new Vector3(transform.position.x, respawnHeight, transform.position.z);
        transform.position = newPosition;
        rb.linearVelocity = Vector3.zero; // Reset velocity
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Check if it hits the player
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 knockbackDirection = (collision.transform.position - transform.position).normalized;
                knockbackDirection.y = 0f; // Slight upward push
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
    }
}
