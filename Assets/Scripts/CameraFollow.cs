using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Assign the player in Inspector
    [SerializeField] private Transform cameraTransform; // Assign the camera object

    [SerializeField] private Vector3 offset = new Vector3(0, 5f, -10f); // Camera offset
    [SerializeField] private float lerpSpeedMultiplier = 5f; // Speed of following

    private Vector3 tempVec3; // Temporary vector for position

    private void LateUpdate()
    {
        if (target != null)
        {
            // Set camera position with an offset
            tempVec3 = target.position + offset;

            // Smoothly follow the target
            transform.position = Vector3.Lerp(transform.position, tempVec3, Time.deltaTime * lerpSpeedMultiplier);

            // Make the camera look at the target
            transform.LookAt(target);
        }
        else if (cameraTransform != null)
        {
            // Keep the camera at its last known position
            tempVec3.x = cameraTransform.position.x;
            tempVec3.y = cameraTransform.position.y;
            tempVec3.z = cameraTransform.position.z;
            cameraTransform.position = tempVec3;
        }
    }
}
