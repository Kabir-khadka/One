using UnityEngine;
using System.Collections.Generic;

public class ShakingPlatform : ObstacleBase
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeSpeed = 10f;
    [SerializeField] private float shakeDuration = 2f;
    [SerializeField] private float timeBetweenShakes = 5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource shakeAudioSource;

    private Vector3 originalPosition;
    private bool isShaking = false;

    // List to keep track of players currently on the platform
    private List<Rigidbody> playersOnPlatform = new List<Rigidbody>();

    private void Start()
    {
        originalPosition = transform.position;
        StartShakingRoutine();
    }

    private void StartShakingRoutine()
    {
        InvokeRepeating(nameof(StartShaking), timeBetweenShakes, timeBetweenShakes + shakeDuration);
    }

    private void StartShaking()
    {
        isShaking = true;

        //PLay the shake sound here when shaking starts
        if(shakeAudioSource != null)
        {
            shakeAudioSource.Play();

            //Optionally stop the sound manually after shakeDuration
            if (shakeAudioSource.clip != null && shakeAudioSource.clip.length > shakeDuration)
            {
                Invoke(nameof(StopShakeSound), shakeDuration);
            }
        }
        else
        {
            Debug.LogWarning("Shake AudioSource is not assign on: " + gameObject.name);
        }
        Invoke(nameof(StopShaking), shakeDuration);
    }

    private void StopShaking()
    {
        isShaking = false;
        transform.position = originalPosition;

        //Stop the shake sound here too (if not already stopped)
        StopShakeSound();
    }

    private void StopShakeSound()
    {
        if (shakeAudioSource != null && shakeAudioSource.isPlaying)
        {
            shakeAudioSource.Stop();
        }
    }

    private void Update()
    {
        if (isShaking)
        {
            float shakeOffsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float shakeOffsetZ = Mathf.Sin(Time.time * shakeSpeed * 1.5f) * shakeIntensity;
            transform.position = originalPosition + new Vector3(shakeOffsetX, 0, shakeOffsetZ);

            // ? Apply force to all players currently on the platform
            ApplyKnockbackToPlayers();
        }
    }

    private void ApplyKnockbackToPlayers()
    {
        foreach (Rigidbody playerRb in playersOnPlatform)
        {
            if (playerRb != null)
            {
                Vector3 knockbackDirection = (playerRb.position - transform.position).normalized;
                knockbackDirection.y = 0.5f; // Slight upward force

                playerRb.AddForce(knockbackDirection * baseKnockbackForce, ForceMode.Impulse);
            }
        }
    }

    // ? Detect when a player steps onto the platform
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null && !playersOnPlatform.Contains(playerRb))
            {
                playersOnPlatform.Add(playerRb);
            }
        }
    }

    // ? Remove player from list when they leave the platform
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null && playersOnPlatform.Contains(playerRb))
            {
                playersOnPlatform.Remove(playerRb);
            }
        }
    }

    protected override float GetKnockbackForce(Rigidbody playerRb)
    {
        return isShaking ? baseKnockbackForce : 0;
    }
}
