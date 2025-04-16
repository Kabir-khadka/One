using UnityEngine;

public class ElectricBullet : MonoBehaviour
{
    [SerializeField] private float speed = 30f;  // Bullet speed
    [SerializeField] private float lifetime = 5f; // Bullet lifetime before auto-destroy
    [SerializeField] private float damage = 25f;  // Damage amount
    [SerializeField] private float impactRadius = 1.5f; // Area damage radius
    //[SerializeField] private GameObject electricImpactEffectPrefab; // Electric impact effect prefab

    private Rigidbody rb;
    private Vector3 direction;
    private AudioSource electricSound;

    void Awake()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        electricSound = GetComponent<AudioSource>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody attached to ElectricBullet.");
        }

        if (electricSound == null)
        {
            Debug.LogWarning("No AudioSource found on ElectricBullet. Please attach one");
        }
    }

    void Start()
    {
        // Set the direction the bullet will travel
        direction = transform.forward;

        // Destroy bullet after 'lifetime' seconds
        Destroy(gameObject, lifetime);

        //Play the electric sound when the bullet is fired
        if (electricSound != null)
        {
            electricSound.Play();
        }
    }

    void Update()
    {
        // Move the bullet manually using position update (for kinematic Rigidbody)
        if (rb != null)
        {
            rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Apply damage to the object hit
        ApplyDamage(collision.gameObject);

        // Stop the sound before destroying (optional)
        if (electricSound != null && electricSound.isPlaying)
        {
            electricSound.Stop();
        }

       /* // Instantiate electric impact effect
        if (electricImpactEffectPrefab != null)
        {
            Instantiate(electricImpactEffectPrefab, transform.position, Quaternion.identity);
        }*/

        // Destroy bullet on impact
        Destroy(gameObject);
    }

    private void ApplyDamage(GameObject target)
    {
        // Here, no health system, so we leave it empty for now.
    }
}
