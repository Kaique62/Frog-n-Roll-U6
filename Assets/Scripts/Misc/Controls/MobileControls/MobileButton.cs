using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mobile button input by detecting pointer down and up events,
/// updating the MobileInput system accordingly.
/// </summary>
public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// The name/key of the input this button represents.
    /// </summary>
    public string inputName;

    /// <summary>
    /// Called when the pointer (touch) is pressed down on this button.
    /// Sets the corresponding input in MobileInput as held (true).
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        MobileInput.Held[inputName] = true;
    }

    /// <summary>
    /// Called when the pointer (touch) is released from this button.
    /// Sets the corresponding input in MobileInput as not held (false).
    /// </summary>
    /// <param name="eventData">Pointer event data.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        MobileInput.Held[inputName] = false;
    }
}
