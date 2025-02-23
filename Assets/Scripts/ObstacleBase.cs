using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [SerializeField] protected float baseKnockbackForce = 50f;

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                float modifiedForce = GetKnockbackForce(playerRb);
                ApplyKnockback(collision.gameObject, modifiedForce);
            }
        }
    }

    protected virtual float GetKnockbackForce(Rigidbody playerRb)
    {
        // Default knockback force, but can be overridden by child classes
        return baseKnockbackForce;
    }

    protected virtual void ApplyKnockback(GameObject player, float force)
    {
        Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
        knockbackDirection.y = 0f; // Add slight upward force

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(knockbackDirection * force, ForceMode.Impulse);
        }
    }
}
