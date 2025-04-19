using UnityEngine;

public class MoveLeftRight : MonoBehaviour
{
    public float moveLeftSpeed = 2f;
    public float moveRightSpeed = 2f;
    public float maxRightDistance = 5f;
    public float maxLeftDistance = 5f;

    public Vector3 initialPosition;
    private bool movingLeft = true;
    private bool movingRight = false;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (movingRight)
        {
            MoveRight();
        }
        else if (movingLeft)
        {
            MoveLeft();
        }
    }

    private void MoveRight()
    {
        transform.position += Vector3.right * moveRightSpeed * Time.deltaTime;
        if (transform.position.x >= initialPosition.x + maxRightDistance)
        {
            transform.position = new Vector3(initialPosition.x + maxRightDistance, transform.position.y, transform.position.z);
            movingRight = false;
            movingLeft = true;
        }
    }

    private void MoveLeft()
    {
        transform.position += Vector3.left * moveLeftSpeed * Time.deltaTime;
        if (transform.position.x <= initialPosition.x - maxLeftDistance)
        {
            transform.position = new Vector3(initialPosition.x - maxLeftDistance, transform.position.y, transform.position.z);
            movingLeft = false;
            movingRight = true;
        }
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
