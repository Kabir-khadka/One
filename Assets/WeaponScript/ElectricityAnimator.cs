using UnityEngine;

public class ElectricityAnimator : MonoBehaviour
{
    private Material material;
    public float speedX = 0.1f;
    public float speedY = 0.05f;
    private Vector2 offset;

    void Start()
    {
        // Get the material instance
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Update the offset based on time
        offset.x = offset.x + speedX * Time.deltaTime;
        offset.y = offset.y + speedY * Time.deltaTime;

        // Apply the offset to the material
        material.SetTextureOffset("_BaseColorMap", offset);
    }

    void OnDestroy()
    {
        // Clean up the material instance
        if (material != null)
            Destroy(material);
    }
}