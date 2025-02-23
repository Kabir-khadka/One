using UnityEngine;
using System.Collections;
public class PunchingWall : MonoBehaviour
{

    [SerializeField] private float pushDistance = 2f; //How far the wall moves forward
    [SerializeField] private float pushSpeed = 2f; // How fast the wall moves
    [SerializeField] private float delayBetweenPunches = 2f; // Time between punches

    private Vector3 originalPosition;
    private bool isPunching = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
        StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayBetweenPunches);
            isPunching = true;

            //Move forward(punch)
            Vector3 targetPosition = originalPosition + transform.forward * pushDistance;
            while(Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, pushSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f); // Pause at max distance

            //Move back to original position
            while(Vector3.Distance(transform.position, originalPosition) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, originalPosition, pushSpeed * Time.deltaTime);
                yield return null;
            }

            isPunching = false;
        }
    }

}
