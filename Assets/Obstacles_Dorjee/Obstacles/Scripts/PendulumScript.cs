using UnityEngine;

public class PendulumScript : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float limit = 75f;
    [SerializeField] private bool randomStart = false;
    [SerializeField] private float random = 0;
    public float angle;

    void Awake()
    {
        if (randomStart)
        {
            random = Random.Range(0f, 1f);
        }
    }

    void Update()
    {
        angle = limit * Mathf.Sin(Time.time + random * speed);
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         Rigidbody rb = other.GetComponent<Rigidbody>();
    //         if (rb != null)
    //         {
    //             Vector3 direction = (other.transform.position - transform.position).normalized;
    //             direction.y = 0.15f;

    //             // Add knockback direction based on pendulum swing
    //             if (angle < 0)
    //             {
    //                 direction.x = -0.5f;
    //             }
    //             else if (angle > 0)
    //             {
    //                 direction.x = 0.5f;
    //             }

    //             float knockbackForce = 30f; // Tweak this for desired strength
    //             rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
    //         }
    //     }
    // }
}
