using UnityEngine;

public class RotateYaxis : MonoBehaviour
{
    public float rotateSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }


     private void OnCollisionEnter(Collision  collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Make sure your player GameObject has the "Player" tag
        {
            collision.transform.SetParent(transform); // Make player a child of the rotating object
            Debug.Log("Player is now a child of the rotating platform.");
        }
    }

    private void OnCollisionExit(Collision  collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null); // Unparent the player
            Debug.Log("Player is no longer a child of the rotating platform.");
        }
    }
}
