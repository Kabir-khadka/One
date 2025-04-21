using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    public GameObject wallLeft;
    public GameObject wallRight;
    public float openDistance = 3f;
    public float openSpeed = 2f;

    private bool isOpening = false;

    private Vector3 wallLeftTargetPos;
    private Vector3 wallRightTargetPos;

    void Start()
    {
        // Set positions to move to when opening
        wallLeftTargetPos = wallLeft.transform.position + Vector3.left * openDistance;
        wallRightTargetPos = wallRight.transform.position + Vector3.right * openDistance;
    }

    void Update()
    {
        if (isOpening)
        {
            wallLeft.transform.position = Vector3.MoveTowards(wallLeft.transform.position, wallLeftTargetPos, openSpeed * Time.deltaTime);
            wallRight.transform.position = Vector3.MoveTowards(wallRight.transform.position, wallRightTargetPos, openSpeed * Time.deltaTime);
        }
    }

    public void TriggerWallOpen()
    {
        isOpening = true;
    }
}
