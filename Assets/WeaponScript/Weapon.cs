using UnityEngine;

public enum WeaponType
{
    Pistol,
    Rifle
}

public class Weapon : MonoBehaviour
{
    private Rigidbody weaponRigidbody;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private WeaponType weaponType;

    public WeaponType WeaponType => weaponType;

    public bool isRotating { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weaponRigidbody = GetComponent<Rigidbody>();

        if (weaponRigidbody)
        {
            weaponRigidbody.isKinematic = true;
        }

        isRotating = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRotating) return;
        transform.Rotate(Vector3.up * rotationSpeed * (1 - Mathf.Exp(-rotationSpeed * Time.deltaTime)));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (weaponRigidbody)
                weaponRigidbody.constraints = RigidbodyConstraints.FreezePosition;

            isRotating = true;
        }
    }

    public void ChangeBehaviour()
    {
        if (weaponRigidbody)
        {
            weaponRigidbody.isKinematic = true;
            weaponRigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}