using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Image joystickBg;
    public Image joystickHandle;
    public float deadZone = 0.2f;

    private Vector2 inputVector;
    private Vector2 startPos;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Captura o ponto inicial do toque no retângulo do joystick
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBg.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out startPos
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBg.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out currentPos
        );

        Vector2 delta = currentPos - startPos;

        delta /= joystickBg.rectTransform.sizeDelta;
        delta = delta * 2; // Normaliza para -1 a 1

        inputVector = Vector2.ClampMagnitude(delta, 1f);
        joystickHandle.rectTransform.anchoredPosition = inputVector * (joystickBg.rectTransform.sizeDelta.x / 2.5f);

        UpdateDirectionInputs(inputVector);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickHandle.rectTransform.anchoredPosition = Vector2.zero;
        ClearDirectionInputs();
    }

    void UpdateDirectionInputs(Vector2 input)
    {
        MobileInput.Held["Left"]  = input.x < -deadZone;
        MobileInput.Held["Right"] = input.x > deadZone;
        //MobileInput.Held["Up"]    = input.y > deadZone;
        MobileInput.Held["Down"]  = input.y < -deadZone;
    }

    void ClearDirectionInputs()
    {
        MobileInput.Held["Left"]  = false;
        MobileInput.Held["Right"] = false;
        MobileInput.Held["Up"]    = false;
        MobileInput.Held["Down"]  = false;
    }

    public Vector2 GetInputDirection() => inputVector;
}
