using UnityEngine;
using System.Collections;

public class CollapsingPlatform : ObstacleBase
{
    [SerializeField] private float collapseDelay = 4f; // Time before collapse
    [SerializeField] private float respawnDelay = 5f; // Time before reset
    [SerializeField] private Material crackedMaterial; // Optional cracked material

    private Material platformMaterial;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isCollapsing = false;
    private float crackStrength = 0f;
    private Renderer mainRenderer;
    private Collider mainCollider;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        mainRenderer = GetComponent<Renderer>();
        mainCollider = GetComponent<Collider>();

        if (mainRenderer != null)
        {
            platformMaterial = mainRenderer.material;
            platformMaterial.SetFloat("_CrackStrength", 0f); // Start with no cracks
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only trigger if it's the player and we're not already collapsing
        if (collision.gameObject.CompareTag("Player") && !isCollapsing)
        {
            Debug.Log("Player collision detected, starting collapse sequence");
            StartCoroutine(CrackingSequence());
        }
    }

    private IEnumerator CrackingSequence()
    {
        isCollapsing = true;
        float elapsedTime = 0f;

        Debug.Log("Starting cracking animation for exactly 4 seconds");

        // Gradual cracking animation - always runs for exactly 4 seconds
        while (elapsedTime < collapseDelay)
        {
            // Calculate new crack strength
            crackStrength = Mathf.Lerp(0f, 1f, elapsedTime / collapseDelay);

            // Apply to material
            if (platformMaterial != null)
            {
                platformMaterial.SetFloat("_CrackStrength", crackStrength);
            }

            // Add slight visual shake as it cracks more
            if (elapsedTime > collapseDelay * 0.7f) // Only in last 30% of time
            {
                float shakeMagnitude = 0.005f * (crackStrength * 2f);
                transform.position = originalPosition + Random.insideUnitSphere * shakeMagnitude;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure full crack visibility
        if (platformMaterial != null)
        {
            platformMaterial.SetFloat("_CrackStrength", 1f);
        }

        Debug.Log("4 second crack animation completed, platform disabled");

        // Disable platform
        if (mainRenderer != null)
        {
            mainRenderer.enabled = false;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        // Wait for respawn
        yield return new WaitForSeconds(respawnDelay);

        // Reset this platform
        ResetPlatform();
    }

    public void ResetPlatform()
    {
        Debug.Log("Resetting platform");

        // Reset crack material
        if (mainRenderer != null && platformMaterial != null)
        {
            platformMaterial.SetFloat("_CrackStrength", 0f);
        }

        // Reset position
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Show main platform
        if (mainRenderer != null)
        {
            mainRenderer.enabled = true;
        }

        // Enable main collider
        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }

        // Reset collapse state
        isCollapsing = false;

        Debug.Log("Platform has been reset");
    }
}
