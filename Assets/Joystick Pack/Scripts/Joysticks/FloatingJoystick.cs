using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class FloatingJoystick : Joystick
{
    [SerializeField] private float activationRadius = 0.6f; // Normalized range: 0 = center, 1 = edge

    private bool isTouchValid = false;

    protected override void Start()
    {
        base.Start();
        background.gameObject.SetActive(true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        isTouchValid = IsWithinActivationRadius(eventData);

        if (isTouchValid)
        {
            base.OnPointerDown(eventData); // This calls base.OnDrag(eventData) too
        }
    }

    public void Update()
    {
        // Optional: if you want to cancel input when user drags too far
        if (!isTouchValid)
        {
            typeof(Joystick).GetField("input", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(this, Vector2.zero);
            handle.anchoredPosition = Vector2.zero;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isTouchValid = false;
    }

    private bool IsWithinActivationRadius(PointerEventData eventData)
    {
        Camera cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        Vector2 joystickCenter = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        float radius = (background.sizeDelta.x * 0.5f) * canvas.scaleFactor;

        float distance = Vector2.Distance(eventData.position, joystickCenter);
        return distance <= radius * activationRadius;
    }

    public void ResetJoystickInput()
    {
        typeof(Joystick).GetField("input", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(this, Vector2.zero);
        handle.anchoredPosition = Vector2.zero;
    }
}
