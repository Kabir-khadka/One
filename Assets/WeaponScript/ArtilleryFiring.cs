using UnityEngine;

public class ArtilleryFiring : MonoBehaviour
{
    // Bullet prefab and firing point
    public GameObject bulletPrefab;
    public Transform firingPoint;

    // Firing parameters
    public float fireRate = 1f;
    public float playerDetectionRadius = 20f;
    public bool autoFire = true;
    public string playerTag = "Player";

    // New variables for freezing effect
    private bool isFrozen = false;
    private float frozenUntilTime = 0f;

    // Optional variables for visual feedback
    public GameObject electricalEffectPrefab; // Visual effect to show when frozen
    private GameObject activeElectricalEffect;

    // Internal variables
    private float nextFireTime = 0f;
    private Transform targetPlayer;
    private AudioSource audioSource; // Cached AudioSource

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on " + gameObject.name);
        }
    }

    void Update()
    {
        // Check if we're still frozen
        if (isFrozen)
        {
            if (Time.time >= frozenUntilTime)
            {
                // Unfreeze when time is up
                isFrozen = false;

                // Remove electrical effect if it exists
                if (activeElectricalEffect != null)
                {
                    Destroy(activeElectricalEffect);
                    activeElectricalEffect = null;
                }
            }
            else
            {
                // Still frozen, don't allow firing
                return;
            }
        }

        // Check if any players are in range
        if (autoFire && Time.time >= nextFireTime)
        {
            if (IsPlayerInRange())
            {
                FireBullet();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }

        // Manual fire option (if you want to trigger it manually)
        if (!autoFire && Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }

    // Public method to freeze the tank weapon (called by ElectricBullet)
    public void Freeze(float duration)
    {
        isFrozen = true;
        frozenUntilTime = Time.time + duration;

        // Optional: Show electrical effect while frozen
        if (electricalEffectPrefab != null && activeElectricalEffect == null)
        {
            activeElectricalEffect = Instantiate(electricalEffectPrefab, transform.position, Quaternion.identity, transform);
        }

        // Optional: Play electrical sound
        if (audioSource != null)
        {
            // You could play a different sound for getting electrocuted
            // audioSource.PlayOneShot(electricalHitSound);
        }

        Debug.Log("Tank weapon frozen for " + duration + " seconds");
    }

    bool IsPlayerInRange()
    {
        // Check for players within detection radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, playerDetectionRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(playerTag))
            {
                // Store the player transform for aiming
                targetPlayer = hitCollider.transform;
                return true;
            }
        }

        targetPlayer = null;
        return false;
    }

    void FireBullet()
    {
        if (bulletPrefab == null || firingPoint == null)
            return;

        // If we have a target, aim toward them
        if (targetPlayer != null)
        {
            // Get direction to player but keep the gun's up orientation
            Vector3 targetDirection = targetPlayer.position - firingPoint.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Instantiate bullet at firing point, facing the player
            GameObject newBullet = Instantiate(bulletPrefab, firingPoint.position, targetRotation);
        }
        else
        {
            // If no target, just fire forward
            GameObject newBullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);
        }

        // Play sound effect
        
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    // Visualization for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        if (firingPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firingPoint.position, 0.2f);
            Gizmos.DrawRay(firingPoint.position, firingPoint.forward * 2);
        }
    }
}