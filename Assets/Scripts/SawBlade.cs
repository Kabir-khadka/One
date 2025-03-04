using UnityEngine;

public class SawBlade : ObstacleBase
{
    [SerializeField] private float rotationSpeed = 300f; // Rotation speed in degrees per second

    // Update is called once per frame
    void Update()
    {
        // Rotate in place like a windmill or ship's wheel
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        // Use the base knockback force but allow for modifications if needed
        return baseKnockbackForce;
    }

    protected override void ApplyKnockback(GameObject player, float force)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            knockbackDirection.y = 0.2f; // Small upward push to make knockback feel better
            playerRb.AddForce(knockbackDirection * force, ForceMode.Impulse);
        }
    }
}
