using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq; // Adicionado para facilitar a cópia da lista

public class ControllerEditor : MonoBehaviour
{
    [Header("References to buttons")]
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

    // NOVO: "Memória" para guardar o estado dos botões ao abrir o editor.
    private ButtonList lastSavedData;

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
        }
    }

    public void LoadConfigurations()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                ApplyDefaultLayoutAndSave();
                return;
            }
            
            ButtonList data = JsonUtility.FromJson<ButtonList>(json);
            if (data == null || data.buttons == null || data.buttons.Count == 0)
            {
                 ApplyDefaultLayoutAndSave();
            }
            else
            {
                ApplyConfigurationData(data);
            }
        }
        else
        {
            ApplyDefaultLayoutAndSave();
        }
    }

    // MUDANÇA: Renomeado para maior clareza
    private void ApplyDefaultLayoutAndSave()
    {
        Debug.Log("No valid configuration found. Creating, applying, and saving default layout.");
        
        // Pega os dados padrão
        ButtonList defaultData = GetDefaultButtonList();
        // Aplica na tela E guarda na "memória"
        ApplyConfigurationData(defaultData);
        // Salva no arquivo
        SaveConfigurations();
    }

    private void ApplyConfigurationData(ButtonList data)
    {
        if (data == null || data.buttons == null) return;

        foreach (ButtonData entry in data.buttons)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.name) && buttons.TryGetValue(entry.name, out RectTransform rt))
            {
                if (entry.position != null) rt.anchoredPosition = entry.position.ToVector2();
                if (entry.size != null) rt.sizeDelta = entry.size.ToVector2();
            }
        }
        
        // NOVO: Guarda uma cópia do estado que acabou de ser carregado/aplicado.
        // Isso serve como nosso ponto de "cancelar/restaurar".
        this.lastSavedData = new ButtonList { buttons = data.buttons.Select(b => new ButtonData { name = b.name, position = b.position, size = b.size }).ToList() };
    }

    // MUDANÇA: Esta função agora só lê os dados da tela e os transforma em uma lista.
    private ButtonList GetCurrentLayoutData()
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
        return new ButtonList { buttons = list };
    }

    // MUDANÇA: Agora usa a função acima para pegar os dados e salvar.
    public void SaveConfigurations()
    {
        ButtonList currentData = GetCurrentLayoutData();
        string json = JsonUtility.ToJson(currentData, true);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, json);
        Debug.Log("Configurations saved to: " + filePath);
    }
    
    // --- FUNÇÕES PÚBLICAS PARA OS BOTÕES ---

    public void SaveAndCloseEditor()
    {
        SaveConfigurations();
        if (controlCanvas != null)
        {
            controlCanvas.SetActive(false);
        }
    }
    
    public void CloseEditor()
    {
        Debug.Log("Closing editor and discarding unsaved changes.");
        // NOVO: Restaura as configurações para o último estado salvo antes de fechar.
        if (lastSavedData != null)
        {
            ApplyConfigurationData(lastSavedData);
        }

        if (controlCanvas != null)
        {
            controlCanvas.SetActive(false);
        }
    }
    
    public void InvertControls()
    {
        Debug.Log("Visually inverting control positions.");
        foreach (var buttonPair in buttons)
        {
            RectTransform rt = buttonPair.Value;
            Vector2 currentPos = rt.anchoredPosition;
            currentPos.x = -currentPos.x;
            rt.anchoredPosition = currentPos;
        }
        // REMOVIDO: Não salva mais automaticamente.
    }

    public void ResetToDefault()
    {
        Debug.Log("Visually resetting controls to default layout.");
        ButtonList defaultData = GetDefaultButtonList();
        // Apenas aplica na tela, sem salvar.
        ApplyConfigurationData(defaultData);
    }

    // NOVO: Função helper para pegar a lista de botões padrão
    private ButtonList GetDefaultButtonList()
    {
        List<ButtonData> defaultButtons = new List<ButtonData>
        {
            new ButtonData { name = "JumpButton", position = new SerializableVector2(new Vector2(814.0f, -373.0f)), size = new SerializableVector2(new Vector2(200.0f, 200.0f)) },
            new ButtonData { name = "RollButton", position = new SerializableVector2(new Vector2(565.0f, -388.0f)), size = new SerializableVector2(new Vector2(200.0f, 200.0f)) },
            new ButtonData { name = "KickButton", position = new SerializableVector2(new Vector2(614.0f, -173.0f)), size = new SerializableVector2(new Vector2(200.0f, 200.0f)) },
            new ButtonData { name = "PunchButton", position = new SerializableVector2(new Vector2(838.0f, -126.0f)), size = new SerializableVector2(new Vector2(200.0f, 200.0f)) },
            new ButtonData { name = "JoystickBackground", position = new SerializableVector2(new Vector2(-671.0f, -325.0f)), size = new SerializableVector2(new Vector2(110.0f, 100.0f)) }
        };
        return new ButtonList { buttons = defaultButtons };
    }
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