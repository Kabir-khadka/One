using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystickSecond : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;

    private Vector2 inputVector = Vector2.zero;
    private Vector2 joystickCenter;
    private float maxRadius;

    // Sprint input event delegate
    public delegate void OnSprintInputChanged(bool isSprinting);
    public event OnSprintInputChanged SprintInputChanged;

    [SerializeField] private float sprintThreshold = 0.7f; // 70% of maxRadius

    private void Start()
    {
        if (background == null)
            background = transform.parent.GetComponent<RectTransform>();

        if (handle == null)
        {
            Transform handleTransform = transform.Find("JoystickHandle");
            handle = handleTransform != null ? handleTransform.GetComponent<RectTransform>() : GetComponent<RectTransform>();
        }

        maxRadius = background.sizeDelta.x / 2;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        background.gameObject.SetActive(true);
        background.position = eventData.position;
        handle.position = eventData.position;
        joystickCenter = eventData.position;

        OnDrag(eventData); // Prevent input lag
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - joystickCenter;
        float distance = direction.magnitude;

        // Clamp input to max radius
        inputVector = (distance > maxRadius) ? direction.normalized : direction / maxRadius;

        // Move handle within boundary
        handle.position = joystickCenter + (inputVector * maxRadius);

        // Sprint logic
        bool isSprinting = distance >= (sprintThreshold * maxRadius);
        SprintInputChanged?.Invoke(isSprinting);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.position = joystickCenter;

        SprintInputChanged?.Invoke(false);
    }

    public Vector2 GetInputDirection()
    {
        return inputVector;
    }
}
