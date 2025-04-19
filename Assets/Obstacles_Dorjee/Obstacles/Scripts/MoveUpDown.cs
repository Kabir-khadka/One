using UnityEngine;

public class MoveUpDown : MonoBehaviour
{
    public float moveUpSpeed = 2f;  // Speed to move up
    public float moveDownSpeed = 2f; // Speed to return down
    public float maxHeight = 5f;    // Maximum height the cube can reach
    private Vector3 initialPosition;
    private bool movingUp = true;
    private bool movingDown = false;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (movingUp)
        {
            MoveUp();
        }
        else if (movingDown)
        {
            MoveDown();
        }
    }

    private void MoveUp()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;
        if (transform.position.y >= initialPosition.y + maxHeight)
        {
            transform.position = new Vector3(transform.position.x, initialPosition.y + maxHeight, transform.position.z);
            movingUp = false;
            movingDown = true; // Start moving down automatically
        }
    }

    private void MoveDown()
    {
        transform.position -= Vector3.up * moveDownSpeed * Time.deltaTime;
        if (transform.position.y <= initialPosition.y)
        {
            transform.position = initialPosition;
            movingDown = false;
            movingUp = true; // Start moving up again
        }
    }
}