using UnityEngine;
using UnityEngine.EventSystems;

public class EditarBotao : MonoBehaviour, IDragHandler
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
}
