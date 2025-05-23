using UnityEngine;
using UnityEngine.EventSystems;

public class EditarBotao : MonoBehaviour, IDragHandler, IScrollHandler
{
    private RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rt.anchoredPosition += eventData.delta;
    }

    // Redimensiona com scroll do mouse (PC)
    public void OnScroll(PointerEventData eventData)
    {
        float delta = eventData.scrollDelta.y * 10f;
        Vector2 novaEscala = rt.sizeDelta + new Vector2(delta, delta);

        novaEscala = Vector2.Max(novaEscala, Vector2.one * 50);   // Mínimo 50x50
        novaEscala = Vector2.Min(novaEscala, Vector2.one * 300);  // Máximo 300x300

        rt.sizeDelta = novaEscala;
    }

#if UNITY_ANDROID || UNITY_IOS
    private bool pinchInProgress = false;
    private float pinchStartDist;
    private Vector2 startSize;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

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

                newSize = Vector2.Max(newSize, Vector2.one * 50);
                newSize = Vector2.Min(newSize, Vector2.one * 300);

                rt.sizeDelta = newSize;
            }
        }
        else
        {
            pinchInProgress = false;
        }
    }
#endif
}
