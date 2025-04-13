using UnityEngine;

public class HoningBullet : MonoBehaviour
{
    // Core bullet properties
    public float speed = 15f;
    public float homingStrength = 8f;
    public float maxLifetime = 8f;
    public float knockbackForce = 25f;
    public float detectionRadius = 15f;

    // References
    private Transform targetPlayer;
    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Find the closest player to target
        FindClosestPlayer();

        // Initial forward velocity
        rb.linearVelocity = transform.forward * speed;

        // Destroy bullet after maximum lifetime to prevent endless bullets
        Destroy(gameObject, maxLifetime);
    }

    void FixedUpdate()
    {
        // If we have a target, adjust course toward them
        if (targetPlayer != null)
        {
            //Aim slightly above the player's feet (e.g., torso height)
            Vector3 targetPosition = targetPlayer.position + Vector3.up * 1.2f;

            // Direction to the target
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Apply homing force toward the target
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, direction * speed, homingStrength * Time.deltaTime);

            // Rotate bullet to face direction of travel
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
        // If target is lost, try to find a new one
        else
        {
            FindClosestPlayer();
        }
    }

    void FindClosestPlayer()
    {
        // Find all players in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            // Check if this is a player (adjust the tag as needed for your project)
            if (hitCollider.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

                // If this player is closer than any we've found so far
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetPlayer = hitCollider.transform;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // If we hit a player, apply knockback
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // Calculate direction for the knockback (away from impact)
                Vector3 knockbackDirection = (collision.transform.position - transform.position).normalized;

                // Apply the force
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

                // Optionally add some upward force for more dramatic effect
                playerRb.AddForce(Vector3.up * knockbackForce * 0.5f, ForceMode.Impulse);

                // You could also apply damage here if you have a health system
                // collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(20);
            }
        }

        // Create an explosion effect here if you want
        // Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Destroy the bullet on impact
        Destroy(gameObject);
    }

    // Visualization for debugging (visible in Scene view)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}