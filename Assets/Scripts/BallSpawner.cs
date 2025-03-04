using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private int ballCount = 10;
    [SerializeField] private GameObject ballPit; //Reference to the ballpit
    [SerializeField] private float spawnHeightOffset = 5f; //Extra height above the surface

    private Vector3 spawnAreaSize;
    private float spawnY;

    private void Start()
    {
        if(ballPit != null)
        {
            BoxCollider ballPitCollider = ballPit.GetComponent<BoxCollider>();
            if(ballPitCollider != null)
            {
                spawnAreaSize = ballPitCollider.bounds.size;
                spawnY = ballPitCollider.bounds.max.y + spawnHeightOffset; ; // Get the top surface Y position
                SpawnBalls();
            }
            else
            {
                Debug.LogError("Ballpit does not have a BoxCollider!");
            }
        }
        else
        {
            Debug.LogAssertionFormat("Ballpit reference is missing!");
        }
        
    }

    private void SpawnBalls()
    {
        for (int i = 0; i < ballCount; i++)
        {
            Vector3 randomPos = new Vector3(
               Random.Range(ballPit.transform.position.x - spawnAreaSize.x / 2, ballPit.transform.position.x + spawnAreaSize.x / 2),
               spawnY,
               Random.Range(ballPit.transform.position.z - spawnAreaSize.z / 2, ballPit.transform.position.z + spawnAreaSize.z / 2)
            );

            Instantiate(ballPrefab, randomPos, Quaternion.identity);
        }
    }
}
