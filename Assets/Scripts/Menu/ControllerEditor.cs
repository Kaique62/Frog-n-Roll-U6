using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Allows editing the position and size of mobile control buttons and saving/loading their configuration.
/// The joystick background is now also editable.
/// </summary>
public class ControllerEditor : MonoBehaviour
{
    [Header("References to buttons to edit")]
    public RectTransform button1;
    public RectTransform button2;
    public RectTransform button3;
    public RectTransform button4;

    [Header("Joystick Background")]
    public RectTransform joystickBackground;

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
        AddButton(joystickBackground);

        LoadConfigurations();
    }

    void AddButton(RectTransform rt)
    {
        if (rt != null)
        {
            buttons[rt.name] = rt;
            if (rt.GetComponent<EditButton>() == null)
            {
                rt.gameObject.AddComponent<EditButton>();
            }
        }
    }

    public void LoadConfigurations()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("Configuration file is empty. Loading default layout.");
                LoadDefaultConfiguration();
                return;
            }
            
            ButtonList data = JsonUtility.FromJson<ButtonList>(json);

            if (data == null || data.buttons == null || data.buttons.Count == 0)
            {
                 Debug.LogWarning("Configuration file contains invalid data. Loading default layout.");
                 LoadDefaultConfiguration();
            }
            else
            {
                ApplyConfigurationData(data);
                Debug.Log("Button configurations loaded from: " + filePath);
            }
        }
        else
        {
            LoadDefaultConfiguration();
        }
    }

    private void LoadDefaultConfiguration()
    {
        Debug.Log("No valid configuration found. Creating and applying default layout.");

        List<ButtonData> defaultButtons = new List<ButtonData>
        {
            new ButtonData { name = "JumpButton", position = new SerializableVector2(new Vector2(546.0f, -217.0f)), size = new SerializableVector2(new Vector2(215.0f, 170.0f)) },
            new ButtonData { name = "RollButton", position = new SerializableVector2(new Vector2(794.0f, -386.0f)), size = new SerializableVector2(new Vector2(216.0f, 170.0f)) },
            new ButtonData { name = "KickButton", position = new SerializableVector2(new Vector2(489.0f, -411.0f)), size = new SerializableVector2(new Vector2(180.0f, 110.0f)) },
            new ButtonData { name = "PunchButton", position = new SerializableVector2(new Vector2(814.0f, -145.0f)), size = new SerializableVector2(new Vector2(180.0f, 170.0f)) },
            new ButtonData { name = "JoystickBackground", position = new SerializableVector2(new Vector2(-680.0f, 0.0f)), size = new SerializableVector2(new Vector2(250.0f, 250.0f)) }
        };

        ButtonList defaultData = new ButtonList { buttons = defaultButtons };

        ApplyConfigurationData(defaultData);
        SaveConfigurations();
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
                if (entry.size != null)
                {
                    rt.sizeDelta = entry.size.ToVector2();
                }
            }
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
                size = new SerializableVector2(rt.sizeDelta)
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
    
    // --- INÍCIO DAS MUDANÇAS ---

    /// <summary>
    /// CORREÇÃO: Inverte o eixo X das posições ATUAIS dos botões, em vez de ler do arquivo de save.
    /// </summary>
    public void InvertControls()
    {
        Debug.Log("Inverting current control positions.");
        
        // Itera pelo dicionário de botões que estão na tela.
        foreach (var buttonPair in buttons)
        {
            RectTransform rt = buttonPair.Value;
            // Pega a posição ATUAL do botão na tela.
            Vector2 currentPos = rt.anchoredPosition;
            // Inverte apenas o eixo X.
            currentPos.x = -currentPos.x;
            // Aplica a nova posição invertida de volta no botão.
            rt.anchoredPosition = currentPos;
        }

        // Depois de inverter todos na tela, salva a nova configuração.
        SaveConfigurations();
        Debug.Log("X axis of controls inverted and saved.");
    }

    /// <summary>
    /// NOVO: Função pública para restaurar as configurações padrão.
    /// Pode ser chamada por um botão de UI.
    /// </summary>
    public void ResetToDefault()
    {
        Debug.Log("Resetting controls to default layout.");
        // Simplesmente chama a função que já tínhamos para carregar o layout padrão.
        LoadDefaultConfiguration();
    }

    // --- FIM DAS MUDANÇAS ---
}


// --- As estruturas de dados continuam as mesmas ---
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