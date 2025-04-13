using UnityEngine;
using System.Collections;

public class DisappearingPlatform : ObstacleBase
{
    [SerializeField] private float disappearDelay = 1.5f; // Time before platform disappears after player steps on it
    [SerializeField] private float reappearDelay = 3f; // Time before platform reappears after disappearing

    private MeshRenderer meshRenderer;
    private Collider platformCollider;
    private bool isDisappearing = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        platformCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with a player
        if (collision.gameObject.CompareTag("Player") && !isDisappearing)
        {
            // Start the disappearing sequence when player steps on platform
            StartCoroutine(DisappearAndReappear());
        }
    }

    private IEnumerator DisappearAndReappear()
    {
        isDisappearing = true;

        // Wait for the disappear delay (player has this much time to move)
        yield return new WaitForSeconds(disappearDelay);

        // Hide the platform
        meshRenderer.enabled = false;
        platformCollider.enabled = false;

        // Wait for the reappear delay
        yield return new WaitForSeconds(reappearDelay);

        // Show the platform again
        meshRenderer.enabled = true;
        platformCollider.enabled = true;

        isDisappearing = false;
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return 0f; // No knockback force needed, the platform just disappears
    }

    protected override void ApplyKnockback(GameObject player, float force)
    {
        // Do nothing, since this obstacle doesn't push the player
    }
}