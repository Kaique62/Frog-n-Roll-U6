using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Allows a UI button to be moved by dragging and resized using scroll (PC) or pinch gesture (mobile).
/// </summary>
public class EditButton : MonoBehaviour, IDragHandler, IScrollHandler
{
    private RectTransform rt;

    private Canvas canvas;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Allows dragging the button on the screen using mouse or touch.
    /// </summary>
    /// <param name="eventData">Pointer event data containing the drag delta.</param>
    public void OnDrag(PointerEventData eventData)
    {
        rt.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    /// <summary>
    /// Resizes the button using mouse scroll wheel (PC only).
    /// </summary>
    /// <param name="eventData">Scroll event data.</param>
    public void OnScroll(PointerEventData eventData)
    {
        float delta = eventData.scrollDelta.y * 10f;
        Vector2 newScale = rt.sizeDelta + new Vector2(delta, delta);

        newScale = Vector2.Max(newScale, Vector2.one * 50);   // Minimum 50x50
        newScale = Vector2.Min(newScale, Vector2.one * 300);  // Maximum 300x300

        rt.sizeDelta = newScale;
    }

    #if UNITY_ANDROID || UNITY_IOS
    private bool pinchInProgress = false;
    private float pinchStartDist;
    private Vector2 startSize;

    /// <summary>
    /// Detects pinch gestures on mobile devices to resize the button.
    /// </summary>
    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            if (RectTransformUtility.RectangleContainsScreenPoint(rt, t1.position, null) &&
                RectTransformUtility.RectangleContainsScreenPoint(rt, t2.position, null))
            {
                float dist = Vector2.Distance(t1.position, t2.position);

                if (!pinchInProgress)
                {
                    pinchStartDist = dist;
                    startSize = rt.sizeDelta;
                    pinchInProgress = true;
                }
                else
                {
                    float scaleFactor = dist / pinchStartDist;
                    Vector2 newSize = startSize * scaleFactor;

                    newSize = Vector2.Max(newSize, Vector2.one * 50);   // Minimum 50x50
                    newSize = Vector2.Min(newSize, Vector2.one * 300);  // Maximum 300x300

                    rt.sizeDelta = newSize;
                }
            }
            else
            {

                pinchInProgress = false;
            }
        }
        else
        {
            pinchInProgress = false;
        }
    }
    #endif
}