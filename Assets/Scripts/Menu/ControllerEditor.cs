using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Allows editing the position and size of mobile control buttons and saving/loading their configuration.
/// It now includes a function to create a default layout if no configuration file is found, if it's empty, or invalid.
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
        AddButton(joystick);

        LoadConfigurations();
    }

    void AddButton(RectTransform rt)
    {
        if (rt != null)
        {
            buttons[rt.name] = rt;
            if (rt.GetComponent<EditButton>() == null && rt != joystick)
            {
                rt.gameObject.AddComponent<EditButton>();
            }
        }
    }

    /// <summary>
    /// Loads configurations. If the file doesn't exist, is empty, or contains invalid data,
    /// it loads a default layout instead.
    /// </summary>
    public void LoadConfigurations()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            // NEW CHECK: If the file is empty or just contains whitespace...
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("Configuration file is empty. Loading default layout.");
                LoadDefaultConfiguration(); // Load defaults, which will also overwrite the empty file.
                return;
            }

            // Attempt to parse the JSON
            ButtonList data = JsonUtility.FromJson<ButtonList>(json);

            // If parsing fails or results in null/empty data, load defaults
            if (data == null || data.buttons == null || data.buttons.Count == 0)
            {
                 Debug.LogWarning("Configuration file contains invalid data. Loading default layout.");
                 LoadDefaultConfiguration();
            }
            else
            {
                // If everything is fine, apply the loaded data
                ApplyConfigurationData(data);
                Debug.Log("Button configurations loaded from: " + filePath);
            }
        }
        else
        {
            // If the file doesn't exist at all, load defaults
            LoadDefaultConfiguration();
        }
    }

    /// <summary>
    /// Creates the default layout you specified, applies it, and saves it to the file.
    /// </summary>
    private void LoadDefaultConfiguration()
    {
        Debug.Log("No valid configuration found. Creating and applying default layout.");

        // Your exact preset is defined here
        List<ButtonData> defaultButtons = new List<ButtonData>
        {
            new ButtonData
            {
                name = "JumpButton",
                position = new SerializableVector2(new Vector2(546.0f, -217.0f)),
                size = new SerializableVector2(new Vector2(215.0f, 170.0f))
            },
            new ButtonData
            {
                name = "RollButton",
                position = new SerializableVector2(new Vector2(794.0f, -386.0f)),
                size = new SerializableVector2(new Vector2(216.0f, 170.0f))
            },
            new ButtonData
            {
                name = "KickButton",
                position = new SerializableVector2(new Vector2(489.0f, -411.0f)),
                size = new SerializableVector2(new Vector2(180.0f, 110.0f))
            },
            new ButtonData
            {
                name = "PunchButton",
                position = new SerializableVector2(new Vector2(814.0f, -145.0f)),
                size = new SerializableVector2(new Vector2(180.0f, 170.0f))
            },
            new ButtonData
            {
                name = "JoystickBackground",
                position = new SerializableVector2(new Vector2(-476.0f, 0.0f)),
                size = null // We set size to null so it's ignored, as per the JSON
            }
        };

        ButtonList defaultData = new ButtonList { buttons = defaultButtons };

        ApplyConfigurationData(defaultData);
        SaveConfigurations(); // This creates/overwrites the file with the correct default data
    }

    private void ApplyConfigurationData(ButtonList data)
    {
        if (data == null || data.buttons == null) return;

        foreach (ButtonData entry in data.buttons)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.name) && buttons.TryGetValue(entry.name, out RectTransform rt))
            {
                if (entry.position != null)
                {
                    rt.anchoredPosition = entry.position.ToVector2();
                }
                if (entry.size != null && rt != joystick)
                {
                    rt.sizeDelta = entry.size.ToVector2();
                }
            }
        }
    }

    public void SaveConfigurations()
    {
        // ... (o resto do seu script continua igual, sem necessidade de alteração)
        List<ButtonData> list = new List<ButtonData>();
        foreach (var item in buttons)
        {
            RectTransform rt = item.Value;
            list.Add(new ButtonData
            {
                name = item.Key,
                position = new SerializableVector2(rt.anchoredPosition),
                size = (rt == joystick) ? null : new SerializableVector2(rt.sizeDelta)
            });
        }
        string json = JsonUtility.ToJson(new ButtonList { buttons = list }, true);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, json);
        Debug.Log("Configurations saved to: " + filePath);
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
            Debug.LogWarning("No file to invert. Loading default layout first might be needed.");
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

        // Apply changes visually
        ApplyConfigurationData(data);
    }
}

// --- Data Structures (No changes needed here) ---

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