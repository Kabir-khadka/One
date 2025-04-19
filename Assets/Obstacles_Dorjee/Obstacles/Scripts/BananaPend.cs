using UnityEngine;

public class BananaPend : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float limit = 75f;
    [SerializeField] private bool randomStart = false;
    [SerializeField] private float random= 0;
    public float angle;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if(randomStart){
            random = Random.Range(0f, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        angle = limit * Mathf.Sin(Time.time + random * speed);
        transform.localRotation = Quaternion.Euler(angle,0, 0);
    }
}
