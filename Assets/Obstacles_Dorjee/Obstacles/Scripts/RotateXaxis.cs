using UnityEngine;

public class RotateXaxis : MonoBehaviour
{
    public float rotateSpeed = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.right * rotateSpeed * Time.deltaTime);
    }
}
