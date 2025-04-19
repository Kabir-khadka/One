using UnityEngine;
using Cinemachine;
using FirstGearGames.SmoothCameraShaker;

public class SpawnedObjectCollision : MonoBehaviour
{
    [SerializeField] private ShakeData collisionShakeData;
    [SerializeField] private float playerRespawnDelay = 2f;
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private GameObject collisionVFXPrefab; // 🎆 VFX Prefab

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 🎆 Spawn VFX at collision point
            if (collisionVFXPrefab != null)
            {
                ContactPoint contact = collision.contacts[0];
                GameObject vfx = Instantiate(collisionVFXPrefab, contact.point, Quaternion.identity);
                Destroy(vfx, 2f); // auto-destroy VFX after 2 seconds
            }

            if (collision.gameObject.CompareTag("Player"))
            {
                // 🎵 Play sound
                if (collisionSound != null)
                {
                    Debug.Log("audio is play");
                    audioSource.PlayOneShot(collisionSound, soundVolume);
                }

                // 💀 Death animation
                Animator animator = collision.gameObject.GetComponent<Animator>();
                if (animator != null)
                    animator.SetTrigger("IsDead");

                // ⏳ Respawn
                PlayerRespawn respawnScript = collision.gameObject.GetComponent<PlayerRespawn>();
                if (respawnScript != null)
                {
                    Debug.Log("delay from SpawnedObjectCollision : " + playerRespawnDelay);
                    respawnScript.TriggerRespawnWithDelay(playerRespawnDelay);
                }
            }

            CameraShakerHandler.Shake(collisionShakeData);
            Destroy(gameObject, 1f); // allow time for audio to finish
        }
    }
}
