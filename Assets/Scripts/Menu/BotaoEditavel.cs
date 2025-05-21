using UnityEngine;
using UnityEngine.EventSystems;

public class BotaoEditavel : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Vector2 offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position, eventData.pressEventCamera, out offset
        );

        offset = rectTransform.anchoredPosition - offset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position, eventData.pressEventCamera, out localMousePosition
        );

        rectTransform.anchoredPosition = localMousePosition + offset;
    }

    // Se quiser permitir redimensionar com as teclas por enquanto (WASD para teste)
    private void Update()
    {
        if (Input.GetKey(KeyCode.KeypadPlus))
            rectTransform.localScale += Vector3.one * 0.01f;
        if (Input.GetKey(KeyCode.KeypadMinus))
            rectTransform.localScale -= Vector3.one * 0.01f;
    }
}
