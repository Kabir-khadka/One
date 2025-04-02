using UnityEngine;
using System.Collections;

public class PlatformMover : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    private bool movingToB = true;
    private bool isFrozen = false;

    void Update()
    {
        if (!isFrozen) // Only move if not frozen
        {
            if (movingToB)
            {
                transform.position = Vector3.MoveTowards(transform.position, pointB.position, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, pointB.position) < 0.1f)
                    movingToB = false;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, pointA.position, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, pointA.position) < 0.1f)
                    movingToB = true;
            }
        }
    }

    public void StartFreeze(float freezeDuration)
    {
        StartCoroutine(FreezePlatform(freezeDuration));
    }

    private IEnumerator FreezePlatform(float duration)
    {
        isFrozen = true;
        yield return new WaitForSeconds(duration);
        isFrozen = false;
    }
}
