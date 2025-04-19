using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;
    private Vector3 currentCheckpoint;
    private Quaternion currentRotation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Set initial checkpoint to player's starting position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentCheckpoint = player.transform.position;
            currentRotation = player.transform.rotation;
        }
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        currentCheckpoint = position;
        currentRotation = rotation;
        Debug.Log("Checkpoint set at: " + position);
    }

    public Vector3 GetCheckpointPosition()
    {
        return currentCheckpoint;
    }

    public Quaternion GetCheckpointRotation()
    {
        return currentRotation;
    }
}