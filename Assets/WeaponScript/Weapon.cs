using UnityEngine;

// Enum representing different types of weapons
public enum WeaponType
{
    Pistol,
    Rifle,
    Ice_Gun,
    Bubble_Gun,
    Electric_Gun
}

public class Weapon : MonoBehaviour
{
    private Rigidbody weaponRigidbody;  // Rigidbody component of the weapon
    [SerializeField] private WeaponType weaponType;  // Type of weapon (set in Inspector)

    // Public property to access the weapon type
    public WeaponType WeaponType => weaponType;

    // Flag to determine if the weapon should rotate
    public bool isRotating { get; set; }

    void Start()
    {
        // Get the Rigidbody component
        weaponRigidbody = GetComponent<Rigidbody>();

        // Ensure the Rigidbody is kinematic initially to prevent physics-based movement
        if (weaponRigidbody)
        {
            weaponRigidbody.isKinematic = true;
        }
    }

    // Trigger event when the weapon collides with another object
    private void OnTriggerEnter(Collider other)
    {
        // If the weapon collides with the ground, freeze its position
        if (other.gameObject.CompareTag("Ground"))
        {
            if (weaponRigidbody)
                weaponRigidbody.constraints = RigidbodyConstraints.FreezePosition;
        }
    }

    // Method to change the weapon's behavior (used when equipping)
    public void ChangeBehaviour()
    {
        if (weaponRigidbody)
        {
            // Enable physics-based movement
            weaponRigidbody.isKinematic = true;
            weaponRigidbody.constraints = RigidbodyConstraints.None;  // Remove movement restrictions
        }
    }
}
