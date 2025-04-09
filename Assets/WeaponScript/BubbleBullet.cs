using UnityEngine;
using System.Collections;

public class BubbleBullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float attachDuration = 3f;
    [SerializeField] private float detectionBuffer = 0.1f;

    [Header("Effects")]
    [SerializeField] private GameObject popEffect;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip attachSound;
    [SerializeField] private AudioClip popSound;

    private Rigidbody rb;
    private bool hasAttached = false;
    private Transform attachedTo;
    private Vector3 attachLocalPosition;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && (shootSound != null || attachSound != null || popSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.volume = 0.7f;
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            PlaySound(shootSound);
        }
    }

    void FixedUpdate()
    {
        if (!hasAttached && rb != null)
        {
            Vector3 direction = rb.linearVelocity.normalized;
            float distance = rb.linearVelocity.magnitude * Time.fixedDeltaTime + detectionBuffer;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, distance))
            {
                AttachAtPosition(hit.transform, hit.point, hit.normal);
            }

            // Debug ray to visualize detection
            Debug.DrawRay(transform.position, direction * distance, Color.red, 0.1f);
        }
    }

    void Update()
    {
        if (hasAttached && attachedTo != null)
        {
            transform.position = attachedTo.TransformPoint(attachLocalPosition);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasAttached)
        {
            AttachToSurface(collision);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasAttached && !other.isTrigger)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position - (transform.forward * 0.1f), transform.forward, out hit, 0.5f))
            {
                AttachAtPosition(hit.transform, hit.point, hit.normal);
            }
            else
            {
                AttachAtPosition(other.transform, transform.position, -transform.forward);
            }
        }
    }

    void AttachToSurface(Collision collision)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        ContactPoint contact = collision.contacts[0];
        transform.position = contact.point + (contact.normal * 0.05f);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

        AttachToObject(collision.transform, contact.point);
        StartCoroutine(PopAfterDelay(attachDuration));
    }

    void AttachAtPosition(Transform hitTransform, Vector3 hitPoint, Vector3 normal)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        transform.position = hitPoint + (normal * 0.05f);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

        AttachToObject(hitTransform, hitPoint);
        StartCoroutine(PopAfterDelay(attachDuration));
    }

    void AttachToObject(Transform newParent, Vector3 worldPosition)
    {
        hasAttached = true;
        attachedTo = newParent;
        attachLocalPosition = newParent.InverseTransformPoint(worldPosition);
        PlaySound(attachSound);
        StartCoroutine(AttachAnimation());
    }

    IEnumerator AttachAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 squishScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.8f, originalScale.z * 1.2f);

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, squishScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(squishScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    IEnumerator PopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PopBubble();
    }

    void PopBubble()
    {
        PlaySound(popSound);

        if (popEffect != null)
        {
            Instantiate(popEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
