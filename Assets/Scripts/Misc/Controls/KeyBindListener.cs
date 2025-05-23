using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyBindListener : MonoBehaviour
{
    private TMP_Text targetText;
    private bool isListening = false;

    public void StartKeyDetection(TMP_Text text)
    {
        if (!isListening)
        {
            targetText = text;
            StartCoroutine(DetectKey());
        }
    }

    private IEnumerator DetectKey()
    {
        isListening = true;
        targetText.text = "...";

        EventSystem.current.SetSelectedGameObject(null);

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                targetText.text = key.ToString();
                break;
            }
        }

        isListening = false;
    }
}
