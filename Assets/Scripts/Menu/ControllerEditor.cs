using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Allows editing the position and size of mobile control buttons and saving/loading their configuration.
/// </summary>
public class ControllerEditor : MonoBehaviour
{
    [Header("References to buttons to edit")]
    public RectTransform button1;
    public RectTransform button2;
    public RectTransform button3;
    public RectTransform button4;

    [Header("Joystick (fixed, not user-editable)")]
    public RectTransform joystick;

    [Header("Canvas containing the buttons (GameCt)")]
    public GameObject controlCanvas;

    private Dictionary<string, RectTransform> buttons = new Dictionary<string, RectTransform>();
    private string filePath;

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "config", "mobile_buttons.json");

        AddButton(button1);
        AddButton(button2);
        AddButton(button3);
        AddButton(button4);
        AddButton(joystick); // ← Joystick is included in the system but won't be draggable

        LoadConfigurations();
    }

    void AddButton(RectTransform rt)
    {
        if (rt != null)
        {
            buttons[rt.name] = rt;

            // Only add the editing component if it's not the joystick
            if (rt.GetComponent<EditButton>() == null && rt != joystick)
                rt.gameObject.AddComponent<EditButton>();
        }
    }

    public void SaveConfigurations()
    {
        List<ButtonData> list = new List<ButtonData>();

        foreach (var item in buttons)
        {
            RectTransform rt = item.Value;
            list.Add(new ButtonData
            {
                name = item.Key,
                position = new SerializableVector2(rt.anchoredPosition),
                size = (rt == joystick) ? null : new SerializableVector2(rt.sizeDelta) // ← Ignore joystick size
            });
        }

        string json = JsonUtility.ToJson(new ButtonList { buttons = list }, true);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, json);
        Debug.Log("Configurations saved to: " + filePath);
    }

    public void LoadConfigurations()
    {
        if (!File.Exists(filePath))
        {
            Debug.Log("No configuration file found.");
            return;
        }

        string json = File.ReadAllText(filePath);
        ButtonList data = JsonUtility.FromJson<ButtonList>(json);

        foreach (ButtonData entry in data.buttons)
        {
            if (buttons.TryGetValue(entry.name, out RectTransform rt))
            {
                rt.anchoredPosition = entry.position.ToVector2();
                if (entry.size != null && rt != joystick)
                    rt.sizeDelta = entry.size.ToVector2();
            }
        }

        Debug.Log("Button configurations loaded.");
    }

    public void SaveAndCloseEditor()
    {
        SaveConfigurations();

        if (controlCanvas != null)
        {
            controlCanvas.SetActive(false);
            Debug.Log("Control editor closed.");
        }
        else
        {
            Debug.LogWarning("Control canvas not assigned.");
        }
    }

    /// <summary>
    /// Inverts the X axis of all button positions saved in the JSON file.
    /// </summary>
    public void InvertControls()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("No file to invert.");
            return;
        }

        string json = File.ReadAllText(filePath);
        ButtonList data = JsonUtility.FromJson<ButtonList>(json);

        foreach (ButtonData entry in data.buttons)
        {
            // Only invert the X axis
            entry.position.x = -entry.position.x;
        }

        string newJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, newJson);
        Debug.Log("X axis of controls inverted and saved.");

        LoadConfigurations(); // Apply changes visually
    }
}

[System.Serializable]
public class ButtonData
{
    public string name;
    public SerializableVector2 position;
    public SerializableVector2 size;
}

[System.Serializable]
public class ButtonList
{
    public List<ButtonData> buttons;
}

[System.Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }

    public Vector2 ToVector2() => new Vector2(x, y);
}
