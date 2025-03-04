using UnityEngine;

public class WallCrusher : MonoBehaviour
{
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float crushThreshold = 1f; // Minimum gap between walls
    [SerializeField] private float grabTime = 1.5f; // Time the player stays grabbed

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;
    private bool movingInward = true;
    private float timer;
    private GameObject trappedPlayer;
    private bool playerGrabbed = false;

    void Start()
    {
        leftStartPos = leftWall.position;
        rightStartPos = rightWall.position;
        timer = waitTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            MoveWalls();
        }
    }

    private void MoveWalls()
    {
        float step = moveSpeed * Time.deltaTime;

        if (movingInward)
        {
            // Move walls toward each other but stop at crushThreshold distance
            float currentDistance = Vector3.Distance(leftWall.position, rightWall.position);
            if (currentDistance > crushThreshold)
            {
                leftWall.position += Vector3.right * step;
                rightWall.position += Vector3.left * step;
            }
            else if (trappedPlayer != null && !playerGrabbed)
            {
                GrabPlayer();
            }

            if (currentDistance <= crushThreshold)
            {
                movingInward = false;
                timer = grabTime; // Stay in grabbing position for a moment
            }
        }
        else
        {
            // Move walls back to original positions
            leftWall.position = Vector3.MoveTowards(leftWall.position, leftStartPos, step);
            rightWall.position = Vector3.MoveTowards(rightWall.position, rightStartPos, step);

            if (Vector3.Distance(leftWall.position, leftStartPos) < 0.1f)
            {
                movingInward = true;
                timer = waitTime;
                ReleasePlayer();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            trappedPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == trappedPlayer)
        {
            trappedPlayer = null;
            playerGrabbed = false;
        }
    }

    private void GrabPlayer()
    {
        if (trappedPlayer != null)
        {
            Rigidbody playerRb = trappedPlayer.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.constraints = RigidbodyConstraints.FreezeAll; // Lock player in place
                playerGrabbed = true;
            }
        }
    }

    private void ReleasePlayer()
    {
        if (trappedPlayer != null)
        {
            Rigidbody playerRb = trappedPlayer.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.constraints = RigidbodyConstraints.FreezeRotation; // Restore movement
            }
            playerGrabbed = false;
        }
    }
}
