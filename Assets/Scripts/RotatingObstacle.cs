using UnityEngine;

public class RotatingObstacle : ObstacleBase
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;  // (0,1,0) for Y-axis rotation
    [SerializeField] private bool eliminatesPlayer = false;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float spinForceMultiplier = 0.5f; // Knockback scales with rotation speed
    [SerializeField] private float maxExtraKnockback = 50f; // Cap additional force

    [Header("Audio Settings")]
    [SerializeField] private AudioSource rotationAudioSource;

    private void Start()
    {
        if (rotationAudioSource != null)
        {
            rotationAudioSource.loop = true;
            rotationAudioSource.playOnAwake = false;

            //Optional: Sync pitch to rotation speed
            rotationAudioSource.pitch = Mathf.Clamp(rotationSpeed / 100f, 0.5f, 2f);

            rotationAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Rotation AudioSource is not assigned on: " + gameObject.name);
        }
    }


    private void Update()
    {
        // Rotate the obstacle around the specified axis
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }


    //Override the base method to modify the knockback force
    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        //Calculate knockback force based on rotation speed
        float spinSpeed = Mathf.Abs(rotationSpeed);
        float force = baseKnockbackForce + (spinSpeed * spinForceMultiplier);

        // Reduce overall knockback by limiting extra force
        return Mathf.Clamp(force, baseKnockbackForce, baseKnockbackForce + maxExtraKnockback);
    }
}