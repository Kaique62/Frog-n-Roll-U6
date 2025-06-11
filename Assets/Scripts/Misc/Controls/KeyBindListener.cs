using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyBindListener : MonoBehaviour
{
    private TMP_Text targetText;
    private bool isListening = false;
    private Coroutine listenCoroutine;

    /// <summary>
    /// Starts listening for a key press and updates the specified TMP_Text with the detected key.
    /// If already listening, it stops the previous detection and starts a new one.
    /// </summary>
    /// <param name="text">The TMP_Text component to update with the detected key name.</param>
    public void StartKeyDetection(TMP_Text text)
    {
        if (isListening)
        {
            StopCoroutine(listenCoroutine);
            isListening = false;
        }

        targetText = text;
        listenCoroutine = StartCoroutine(DetectKey());
    }

    /// <summary>
    /// Cancels the current key detection if it is running.
    /// Updates the TMP_Text with "Cancelled" message.
    /// </summary>
    public void CancelDetection()
    {
        if (isListening)
        {
            StopCoroutine(listenCoroutine);
            isListening = false;
            if (targetText != null)
                targetText.text = "Cancelled";
        }
    }

    /// <summary>
    /// Coroutine that waits for any key press and updates the TMP_Text with the pressed key's name.
    /// Clears any selected UI element at the start to prevent input conflicts.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator DetectKey()
    {
        isListening = true;
        if (targetText != null) targetText.text = "...";

        // Deselect any selected UI element to avoid input conflicts
        EventSystem.current.SetSelectedGameObject(null);

        // Wait until any key is pressed
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        // Detect which key was pressed
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                if (targetText != null)
                    targetText.text = key.ToString();
                break;
            }
        }

        isListening = false;
    }
}
