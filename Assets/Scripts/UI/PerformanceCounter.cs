using UnityEngine;
using TMPro;
using System.IO;          // Required for file operations (Path, File)
using System.Collections.Generic; // Required for List<T>
using System.Linq;        // Required for modern collection queries (FirstOrDefault)

// --- Data classes to represent the structure of your JSON file ---
// These classes allow Unity's JsonUtility to parse the file content.

[System.Serializable]
public class ConfigEntry
{
    // The field names must exactly match the JSON keys ("key" and "value").
    public string key;
    public string value;
}

[System.Serializable]
public class ConfigData
{
    // The field name must be "Values" to match the list in the JSON.
    public List<ConfigEntry> Values;
}


// --- Main FPSCounter component ---

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

    /// <summary>
    /// This method is called when the script instance is being loaded.
    /// It handles loading the configuration from the JSON file to decide
    /// whether the FPS counter should be visible.
    /// </summary>
    void Start()
    {
        // Default behavior: hide the FPS counter. It will only be enabled
        // if the configuration file explicitly says to show it.
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(false);
        }

        // --- New logic to read from config.json ---

        // 1. Define the path to the config.json file.
        string path = Path.Combine(Application.persistentDataPath, "config.json");
        Debug.Log("Attempting to read configuration from: " + path);

        // 2. Check if the configuration file actually exists.
        if (File.Exists(path))
        {
            // 3. Read the entire file content into a string.
            string jsonContent = File.ReadAllText(path);
            
            // 4. Deserialize the JSON string into our C# data objects.
            ConfigData configData = JsonUtility.FromJson<ConfigData>(jsonContent);

            if (configData != null && configData.Values != null)
            {
                // 5. Search for the "mostrarFps" key within the list of values.
                // We use LINQ's FirstOrDefault() for a clean and efficient search.
                ConfigEntry fpsSetting = configData.Values.FirstOrDefault(entry => entry.key == "mostrarFps");

                // 6. If the setting was found, check its value.
                if (fpsSetting != null)
                {
                    Debug.Log("Found 'mostrarFps' setting with value: " + fpsSetting.value);
                    
                    // The value is a string, so we compare it to "True".
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

    /// <summary>
    /// This method is called once per frame.
    /// It calculates and updates the FPS display at the specified interval.
    /// </summary>
    void Update()
    {
        // Optimization: If the FPS text object is disabled, there's no need to perform calculations.
        if (fpsText == null || !fpsText.gameObject.activeInHierarchy)
        {
            return;
        }

        // Accumulate time and frame counts
        timeSinceLastUpdate += Time.deltaTime;
        frameCount++;

        // Check if the update interval has been reached
        if (timeSinceLastUpdate >= updateInterval)
        {
            // Calculate frames per second
            fps = frameCount / timeSinceLastUpdate;

            // Update the UI text
            fpsText.text = "FPS: " + Mathf.Floor(fps).ToString();
            
            // Reset counters for the next interval
            timeSinceLastUpdate = 0f;
            frameCount = 0;
        }
    }
}