using UnityEngine;
using UnityEngine.EventSystems;

public class CameraDragHandler : MonoBehaviour, IDragHandler
{
    public Transform cameraTransform; // Assign in inspector
    public float sensitivity = 0.1f;

    public void OnDrag(PointerEventData eventData)
    {
        float rotateX = eventData.delta.x * sensitivity;
        cameraTransform.Rotate(Vector3.up, rotateX);
    }
}
