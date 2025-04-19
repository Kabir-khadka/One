using UnityEngine;
using System.Collections;

public class ObjectFalling : MonoBehaviour
{
    private bool isPlayerInside = false;

    [SerializeField] private float height = 5.0f;
    [SerializeField] private float groundheight = 0.8f;
    [SerializeField] private float timeInterval = 0.1f;

    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private GameObject indicatorPrefab;

    private GameObject spawnedObject;
    private GameObject spawnedIndicator;
    private Transform playerTransform;
    private Coroutine spawnCycle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerTransform = other.transform;
            spawnCycle = StartCoroutine(SpawnAndCycle());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (spawnCycle != null)
                StopCoroutine(spawnCycle);

            if (spawnedObject != null)
                Destroy(spawnedObject);

            if (spawnedIndicator != null)
                Destroy(spawnedIndicator);
        }
    }

    private IEnumerator SpawnAndCycle()
    {
        while (isPlayerInside)
        {
            // Destroy previous if still around
            if (spawnedObject != null)
                Destroy(spawnedObject);
            if (spawnedIndicator != null)
                Destroy(spawnedIndicator);

            // Spawn indicator
            Vector3 indicatorPosition = new Vector3(playerTransform.position.x, groundheight, playerTransform.position.z);
            spawnedIndicator = Instantiate(indicatorPrefab, indicatorPosition, Quaternion.identity);

            // Spawn falling object
            Vector3 objectPosition = new Vector3(playerTransform.position.x, height, playerTransform.position.z);
            spawnedObject = Instantiate(objectToSpawn, objectPosition, Quaternion.identity);
            Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();

            // Disable, wait, then enable gravity
            spawnedObject.SetActive(false);
            yield return new WaitForSeconds(timeInterval);
            spawnedObject.SetActive(true);
            if (rb != null)
            {
                rb.useGravity = true;
            }

            yield return new WaitForSeconds(timeInterval);
        }
    }
}
