using UnityEngine;
using TMPro;
using System.IO;          // Required for file operations
using System.Linq;        // Required for FirstOrDefault

// Esta é a classe principal que pode ser anexada a um GameObject.
public class FPSCounter : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshPro UI element to display the FPS count.")]
    public TMP_Text fpsText;

    [Header("Performance Settings")]
    [Tooltip("How often the FPS text should be updated (in seconds).")]
    public float updateInterval = 0.5f;

    // Private variables for FPS calculation
    private float timeSinceLastUpdate = 0f;
    private int frameCount = 0;
    private float fps = 0f;

    void Start()
    {
        // Default behavior: hide the FPS counter
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(false);
        }

        // --- Logic to read from config.json ---
        string path = Path.Combine(Application.persistentDataPath, "config.json");
        Debug.Log("Attempting to read configuration from: " + path);

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            
            // O Unity vai encontrar as classes ConfigData e ConfigEntry no outro arquivo
            ConfigData configData = JsonUtility.FromJson<ConfigData>(jsonContent);

            if (configData != null && configData.Values != null)
            {
                ConfigEntry fpsSetting = configData.Values.FirstOrDefault(entry => entry.key == "mostrarFps");

                if (fpsSetting != null)
                {
                    Debug.Log("Found 'mostrarFps' setting with value: " + fpsSetting.value);
                    bool shouldShow = fpsSetting.value == "True";

                    if (fpsText != null)
                    {
                        fpsText.gameObject.SetActive(shouldShow);
                    }
                }
                else
                {
                    Debug.LogWarning("'mostrarFps' key not found in config.json. Counter will remain disabled.");
                }
            }
        }
        else
        {
            Debug.LogWarning("config.json file not found. FPS counter will remain disabled.");
        }
    }

    void Update()
    {
        if (fpsText == null || !fpsText.gameObject.activeInHierarchy)
        {
            return;
        }

        timeSinceLastUpdate += Time.deltaTime;
        frameCount++;

        if (timeSinceLastUpdate >= updateInterval)
        {
            fps = frameCount / timeSinceLastUpdate;
            fpsText.text = "FPS: " + Mathf.Floor(fps).ToString();
            
            timeSinceLastUpdate = 0f;
            frameCount = 0;
        }
    }
}