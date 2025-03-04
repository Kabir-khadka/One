using UnityEngine;

public class SlipperyIce : ObstacleBase
{
    [SerializeField] private float slipperyDrag = 0.1f; //Low frcition value
    [SerializeField] private float exitDrag = 5f; //Normal friction when leaving ice

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if(playerRb != null)
            {
                playerRb.linearDamping = slipperyDrag; // Reduce friction to make it slippery
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if(playerRb != null)
            {
                playerRb.linearDamping = exitDrag; // Restore normal friction
            }
        }
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return 0f; // No knockback, just sliding effect
    }

    protected override void ApplyKnockback(GameObject player, float force)
    {
        // No knockback effect needed
    }
}
