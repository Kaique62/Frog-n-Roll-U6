using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages persistent configuration settings using a JSON file stored on disk.
/// Stores key-value pairs as strings and allows reading and writing.
/// </summary>
public static class ConfigManager
{
    /// <summary>
    /// Full path to the configuration JSON file.
    /// </summary>
    private static string configPath => Path.Combine(Application.persistentDataPath, "config.json");

    /// <summary>
    /// Internal dictionary containing the loaded configuration.
    /// </summary>
    private static Dictionary<string, string> configurations = null;

    /// <summary>
    /// Loads the configuration from the file if not already loaded.
    /// </summary>
    private static void LoadConfigurations()
    {
        if (configurations != null) return;

        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);
            configurations = new Dictionary<string, string>();

            if (wrapper != null && wrapper.Values != null)
            {
                foreach (var item in wrapper.Values)
                {
                    configurations[item.key] = item.value;
                }
            }
        }
        else
        {
            configurations = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Reads the value associated with a given key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The value associated with the key, or null if not found.</returns>
    public static string Read(string key)
    {
        LoadConfigurations();
        return configurations.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Saves or updates a configuration value associated with a given key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The value to be saved.</param>
    public static void Save(string key, string value)
    {
        LoadConfigurations();
        configurations[key] = value;
        SaveToFile();
    }

    /// <summary>
    /// Writes all configuration data to disk in JSON format.
    /// </summary>
    private static void SaveToFile()
    {
        Wrapper wrapper = new Wrapper
        {
            Values = new List<ConfigItem>()
        };

        foreach (var kvp in configurations)
        {
            wrapper.Values.Add(new ConfigItem { key = kvp.Key, value = kvp.Value });
        }

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(configPath, json);
    }

    /// <summary>
    /// Serializable helper class that represents a configuration item (key and value).
    /// </summary>
    [System.Serializable]
    private class ConfigItem
    {
        public string key;
        public string value;
    }

    /// <summary>
    /// Wrapper class for serializing the list of configuration items.
    /// </summary>
    [System.Serializable]
    private class Wrapper
    {
        public List<ConfigItem> Values = new List<ConfigItem>();
    }
}
