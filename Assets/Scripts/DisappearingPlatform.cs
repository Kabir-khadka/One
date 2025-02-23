using UnityEngine;
using System.Collections;

public class DisappearingPlatform : ObstacleBase
{
    [SerializeField] private float disappearDelay = 1.5f; // Time before platform disappears
    [SerializeField] private float reappearDelay = 3f; // Time before platform reappears

    private MeshRenderer meshRenderer;
    private Collider platformCollider;
    private bool isDisappeared = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        platformCollider = GetComponent<Collider>();

        // Start the automatic disappearance and reappearance
        StartCoroutine(AutoDisappearAndReappear());
    }

    private IEnumerator AutoDisappearAndReappear()
    {
        while (true) // Keep running indefinitely
        {
            // Hide the platform
            isDisappeared = true;
            meshRenderer.enabled = false;
            platformCollider.enabled = false;

            // Wait for the disappear delay
            yield return new WaitForSeconds(disappearDelay);

            // Show the platform again
            isDisappeared = false;
            meshRenderer.enabled = true;
            platformCollider.enabled = true;

            // Wait for the reappear delay
            yield return new WaitForSeconds(reappearDelay);
        }
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
