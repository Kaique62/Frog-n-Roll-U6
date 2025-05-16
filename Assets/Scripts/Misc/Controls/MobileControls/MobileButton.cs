using UnityEngine;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string inputName;

    public void OnPointerDown(PointerEventData eventData)
    {
        MobileInput.Held[inputName] = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MobileInput.Held[inputName] = false;
    }
}
