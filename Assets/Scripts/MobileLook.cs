using UnityEngine;

public class MobileLook : MonoBehaviour
{

    public float sensitivity = 0.2f;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;


    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isDragging = true;
            }
            else if(touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touch.deltaPosition * sensitivity;
                transform.Rotate(-delta.y, delta.x, 0);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
    }
}
