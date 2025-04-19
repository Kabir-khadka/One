using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Optional visual to show when checkpoint is activated")]
    public GameObject activeVisual;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Save position slightly above the checkpoint to prevent clipping
            Vector3 safePosition = transform.position + Vector3.up * 0.5f;
            CheckpointManager.Instance.SetCheckpoint(safePosition, other.transform.rotation);

            // Activate visual if assigned
            if (activeVisual != null)
            {
                activeVisual.SetActive(true);
            }
        }
    }
}