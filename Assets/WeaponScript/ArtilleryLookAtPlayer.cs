using UnityEngine;

public class ArtilleryLookAtPlayer : MonoBehaviour
{
    public Transform target; // Drag your player transform here
    public float rotationSpeed = 5f;

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // Keep rotation horizontal (optional, depends on your setup)

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
