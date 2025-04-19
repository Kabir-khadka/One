using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnDelay = 1.0f;
    public string waterTag = "Water";

    [Header("Effects")]
    public ParticleSystem splashEffect;
    public AudioClip splashSound;

    // Private variables - ensure these are declared ONCE at the class level
    private Rigidbody rb;
    private AudioSource audioSource;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isRespawning;

    public AudioClip jumpClip;


    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // Create audio source if needed
        if (audioSource == null && splashSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Store initial position
        startPosition = transform.position;
        startRotation = transform.rotation;

        // Initialize flag
        isRespawning = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check for water collision
        if (other.CompareTag(waterTag) && !isRespawning)
        {
            isRespawning = true;
  
            StartCoroutine(RespawnSequence());
        }
    }

    // Add this method to your PlayerRespawn script
    public void TriggerRespawnWithDelay(float delay)
    {
        respawnDelay = delay;  // Update the delay with the passed value
        StartCoroutine(RespawnSequence());
    }

    IEnumerator RespawnSequence()
    {
        // Set flag
        isRespawning = true;

        // Inform the AnimandMovement script about respawning state
        AnimandMovement animMovement = GetComponent<AnimandMovement>();
        if (animMovement != null)
        {
            // This will use the public field we'll add to AnimandMovement
            animMovement.isRespawning = true;
        }

        // Play splash effect
        if (splashEffect != null)
        {
            Instantiate(splashEffect, transform.position, Quaternion.identity);
        }

        // Play splash sound
        if (audioSource != null && splashSound != null)
        {
            audioSource.PlayOneShot(splashSound);
        }

        // Disable physics
        rb.isKinematic = true;

        // Hide renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        // Wait before respawning
        yield return new WaitForSeconds(respawnDelay);

        // Determine respawn position
        Vector3 respawnPosition;
        Quaternion respawnRotation;

        if (CheckpointManager.Instance != null)
        {
            respawnPosition = CheckpointManager.Instance.GetCheckpointPosition();
            respawnRotation = CheckpointManager.Instance.GetCheckpointRotation();
        }
        else
        {
            respawnPosition = startPosition;
            respawnRotation = startRotation;
        }

        //Add small random offset to avoid overlapping with other players
        Vector3 offset = new Vector3(

            Random.Range(-0.5f, 0.5f),
            0,
            Random.Range(-0.5f, 0.5f)
            );

        // Move player
        transform.position = respawnPosition + offset;
        transform.rotation = respawnRotation;

        // Reset physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Show renderers
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        // Re-enable physics
        rb.isKinematic = false;

        // Clear flag
        isRespawning = false;

        // At the end of respawning, reset the flag in AnimandMovement
        if (animMovement != null)
        {
            animMovement.isRespawning = false;
        }

        // Clear flag
        isRespawning = false;
    }
}