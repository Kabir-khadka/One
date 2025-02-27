using UnityEngine;
using System.Collections;

public class CollapsingPlatform : ObstacleBase
{
    [SerializeField] private float collapseDelay = 2f; // Time before falling
    [SerializeField] private float respawnDelay = 5f; // Time before reset

    private Material platformMaterial; // Unique material for this platform
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isCollapsing = false;
    private float crackStrength = 0f; // Starts with no cracks

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Get the unique material instance from the renderer
        platformMaterial = GetComponent<Renderer>().material;

        if (platformMaterial != null)
        {
            platformMaterial.SetFloat("_CrackStrength", 0f); // Ensure cracks are invisible at start
            Debug.Log("Initial Crack Strength: " + platformMaterial.GetFloat("_CrackStrength"));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCollapsing)
        {
            StartCoroutine(CrackBeforeCollapse());
        }
    }

    private IEnumerator CrackBeforeCollapse()
    {
        isCollapsing = true;

        float elapsedTime = 0f;
        float duration = collapseDelay; // Time for cracks to fully appear

        while (elapsedTime < duration)
        {
            crackStrength = Mathf.Lerp(0f, 1f, elapsedTime / duration); // Animate cracks
            platformMaterial.SetFloat("_CrackStrength", crackStrength); // Update Shader
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        platformMaterial.SetFloat("_CrackStrength", 1f); // Ensure full crack visibility

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(CollapseAndRespawn());
    }

    private IEnumerator CollapseAndRespawn()
    {
        yield return new WaitForSeconds(0.2f);

        GetComponent<Collider>().enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false; // Make it fall

        yield return new WaitForSeconds(respawnDelay);

        rb.isKinematic = true; // Reset physics
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        GetComponent<Collider>().enabled = true;

        // Reset cracks when respawned
        platformMaterial.SetFloat("_CrackStrength", 0f);

        isCollapsing = false;
    }
}
