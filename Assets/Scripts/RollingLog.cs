using UnityEngine;

public class RollingLog : ObstacleBase
{
    [Header("Move Settings")]
    [SerializeField] private float moveDistance = 5f; //How far it moves
    [SerializeField] private float moveSpeed = 2f; // Speed of Movement
    [SerializeField] private float knockbackForce = 35f; //Knockback power

    [Header("Audio Settings")]
    [SerializeField] private AudioSource rollAudioSource;

    private Vector3 startPos;
    private bool movingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Move the log back and forth
        float moveStep = moveSpeed * Time.deltaTime;
        transform.position += (movingRight ? Vector3.right : Vector3.left) * moveStep;

        //Play the rolling sound if its not already playing
        if (rollAudioSource != null && !rollAudioSource.isPlaying)
        {
            rollAudioSource.Play();
        }

        // Check if it should switch direction
        if(Vector3.Distance(transform.position, startPos) >= moveDistance - 0.01f)
        {
            movingRight = !movingRight;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if(playerRb != null)
            {
                ApplyKnockback(collision.gameObject, knockbackForce);
            }
        }
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return knockbackForce;
    }

    protected override void ApplyKnockback(GameObject player, float force)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if(playerRb != null)
        {
            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            knockbackDirection.y = 0.5f; //Slight upward push
            playerRb.AddForce(knockbackDirection * force, ForceMode.Impulse);
        }
    }
}
