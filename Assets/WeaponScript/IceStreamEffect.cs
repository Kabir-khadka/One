using UnityEngine;

public class IceStreamEffect : MonoBehaviour
{
    public float freezeDuration = 3f; // Duration of freeze effect
    [SerializeField] private Collider iceCollider; // Assign manually in Inspector
    [SerializeField] private ParticleSystem iceParticles; // Assign Ice_Stream ParticleSystem manually

    private void Start()
    {
        iceCollider = GetComponent<Collider>();
        iceParticles = GetComponent<ParticleSystem>();

        // Start with the collider disabled
        if (iceCollider != null)
        {
            iceCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetButton("Fire1")) // If Fire1 is held (shooting Ice)
        {
            if (iceCollider != null) iceCollider.enabled = true;
        }
        else
        {
            if (iceCollider != null) iceCollider.enabled = false;
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Pipe")) // Ensure the platform has this tag
        {
            PlatformMover platformMover = other.GetComponent<PlatformMover>();
            if (platformMover != null)
            {
                platformMover.StartFreeze(freezeDuration); // Call freeze function
            }
        }
    }

    private void OnTriggerEnter(Collider other) // Works if using a Collider
    {
        if (other.CompareTag("Pipe"))
        {
            PlatformMover platformMover = other.GetComponent<PlatformMover>();
            if (platformMover != null)
            {
                platformMover.StartFreeze(freezeDuration);
            }
        }
    }
}
