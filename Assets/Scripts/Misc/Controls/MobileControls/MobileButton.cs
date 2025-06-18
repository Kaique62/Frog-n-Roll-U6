using UnityEngine;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Fica 'true' enquanto o botão está pressionado
    public bool IsPressed { get; private set; }

    // Fica 'true' APENAS no primeiro quadro em que o botão é pressionado
    public bool IsDown { get; private set; }

    private bool wasPressedLastFrame;

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }

    // LateUpdate roda depois do Update normal, ideal para calcular inputs de um quadro
    private void LateUpdate()
    {
        IsDown = IsPressed && !wasPressedLastFrame;
        wasPressedLastFrame = IsPressed;
    }
}