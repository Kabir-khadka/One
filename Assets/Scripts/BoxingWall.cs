using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoxingWall : ObstacleBase
{
    [SerializeField] private List<Transform> fists = new List<Transform>(); // Multiple fists
    [SerializeField] private float punchDistance = 2f; // How far they extend
    [SerializeField] private float punchSpeed = 5f;
    [SerializeField] private float retractSpeed = 3f;
    [SerializeField] private float minPunchInterval = 1.5f; // Minimum random interval
    [SerializeField] private float maxPunchInterval = 3f; // Maximum random interval
    [SerializeField] private float knockbackForce = 25f;

    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();

    private void Start()
    {
        foreach (Transform fist in fists)
        {
            originalPositions[fist] = fist.localPosition;
            StartCoroutine(PunchLoop(fist)); // Start a separate loop for each fist
        }
    }

    private IEnumerator PunchLoop(Transform fist)
    {
        while (true)
        {
            float randomInterval = Random.Range(minPunchInterval, maxPunchInterval);
            yield return new WaitForSeconds(randomInterval);

            // Punch outward
            Vector3 targetPosition = originalPositions[fist] + new Vector3(0, 0, punchDistance);
            yield return MoveFist(fist, targetPosition, punchSpeed);

            // Short delay before retracting
            yield return new WaitForSeconds(0.5f);

            // Retract
            yield return MoveFist(fist, originalPositions[fist], retractSpeed);
        }
    }

    private IEnumerator MoveFist(Transform fist, Vector3 target, float speed)
    {
        while (Vector3.Distance(fist.localPosition, target) > 0.01f)
        {
            fist.localPosition = Vector3.MoveTowards(fist.localPosition, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                ApplyKnockback(other.gameObject, knockbackForce);
            }
        }
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return knockbackForce;
    }

    protected override void ApplyKnockback(GameObject player, float force)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            knockbackDirection = new Vector3(knockbackDirection.x, 0, knockbackDirection.z);
            knockbackDirection += Vector3.up * 0.5f; // Slight upward push
            playerRb.AddForce(knockbackDirection.normalized * force, ForceMode.Impulse);
        }
    }
}
