using UnityEngine;

public class ElectricBullet : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float impactRadius = 1.5f;
    public float freezeDuration = 6f;
    [SerializeField] private float raycastDistance = 1f; // Adjust for bullet speed

    private Rigidbody rb;
    private Vector3 direction;
    private AudioSource electricSound;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        electricSound = GetComponent<AudioSource>();

        if (rb == null)
            Debug.LogError("No Rigidbody attached to ElectricBullet.");

        if (electricSound == null)
            Debug.LogWarning("No AudioSource found on ElectricBullet.");
    }

    void Start()
    {
        direction = transform.forward;
        Destroy(gameObject, lifetime);

        if (electricSound != null)
            electricSound.Play();
    }

    void Update()
    {
        // Move bullet
        if (rb != null)
        {
            rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
        }

        // Raycast forward for early detection
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, raycastDistance))
        {
            ArtilleryFiring tankWeapon = hit.collider.GetComponent<ArtilleryFiring>();
            if (tankWeapon != null)
            {
                tankWeapon.Freeze(freezeDuration);
                Debug.Log("Hit tank weapon with raycast!");
                HandleImpact();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        ArtilleryFiring tankWeapon = collision.gameObject.GetComponent<ArtilleryFiring>();
        if (tankWeapon != null)
        {
            tankWeapon.Freeze(freezeDuration);
            Debug.Log("Hit tank weapon with collision!");
        }

        ApplyDamage(collision.gameObject);
        HandleImpact();
    }

    private void HandleImpact()
    {
        if (electricSound != null && electricSound.isPlaying)
            electricSound.Stop();

        Destroy(gameObject);
    }

    private void ApplyDamage(GameObject target)
    {
        // Optional: Apply damage if health system exists
    }
}
