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