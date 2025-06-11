using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the behavior of a mobile joystick UI element.
/// Tracks user drag input to simulate joystick movement and updates directional input states accordingly.
/// </summary>
public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    /// <summary>
    /// Background image of the joystick.
    /// </summary>
    public Image joystickBg;

    /// <summary>
    /// Joystick handle image that moves within the background.
    /// </summary>
    public Image joystickHandle;

    /// <summary>
    /// Dead zone threshold below which input is ignored to prevent unintentional movement.
    /// </summary>
    public float deadZone = 0.2f;

    // The current normalized input vector from -1 to 1 on x and y axes.
    private Vector2 inputVector;

    // The initial local position of the pointer when touching the joystick area.
    private Vector2 startPos;

    /// <summary>
    /// Called when pointer is pressed down on joystick area.
    /// Captures the initial local position of the pointer relative to the joystick background.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBg.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out startPos
        );
    }

    /// <summary>
    /// Called while the pointer is dragging on the joystick area.
    /// Calculates the input vector based on pointer movement and updates the joystick handle position.
    /// Also updates directional inputs for movement.
    /// </summary>
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

        // Normalize delta to range -1 to 1
        delta /= joystickBg.rectTransform.sizeDelta;
        delta *= 2f;

        inputVector = Vector2.ClampMagnitude(delta, 1f);

        // Move the joystick handle visually according to input vector, scaled by background size
        joystickHandle.rectTransform.anchoredPosition = inputVector * (joystickBg.rectTransform.sizeDelta.x / 2.5f);

        // Update directional inputs based on input vector and dead zone
        UpdateDirectionInputs(inputVector);
    }

    /// <summary>
    /// Called when the pointer is released from the joystick.
    /// Resets input vector and joystick handle position, clearing directional inputs.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickHandle.rectTransform.anchoredPosition = Vector2.zero;
        ClearDirectionInputs();
    }

    /// <summary>
    /// Updates the held state of directional inputs based on the current joystick input vector.
    /// </summary>
    /// <param name="input">Normalized input vector from the joystick.</param>
    void UpdateDirectionInputs(Vector2 input)
    {
        MobileInput.Held["Left"] = input.x < -deadZone;
        MobileInput.Held["Right"] = input.x > deadZone;
        MobileInput.Held["Up"] = input.y > deadZone;
        MobileInput.Held["Down"] = input.y < -deadZone;
    }

    /// <summary>
    /// Clears the directional inputs held states (sets them to false).
    /// </summary>
    void ClearDirectionInputs()
    {
        MobileInput.Held["Left"] = false;
        MobileInput.Held["Right"] = false;
        MobileInput.Held["Up"] = false;
        MobileInput.Held["Down"] = false;
    }

    /// <summary>
    /// Returns the current input direction vector of the joystick.
    /// </summary>
    public Vector2 GetInputDirection() => inputVector;
}
