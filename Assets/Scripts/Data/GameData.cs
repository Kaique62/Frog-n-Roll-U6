using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class GameData
{
    private static string dataPath => Application.persistentDataPath;

    // Saves the dictionary to a relative path (e.g., "config/KeyBinds.json")
    public static void Save(Dictionary<string, Dictionary<string, string>> data, string relativePath)
    {
        string fullPath = Path.Combine(dataPath, relativePath);
        string dirPath = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(fullPath, json);

        Debug.Log("Saved to: " + fullPath);
    }

    // Loads the dictionary from a relative path (e.g., "config/KeyBinds.json")
    public static Dictionary<string, Dictionary<string, string>> Load(string relativePath)
    {
        string fullPath = Path.Combine(dataPath, relativePath);

        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        }
        else
        {
            Debug.LogWarning("Data not found at: " + fullPath);
            return null;
        }
    }

    // Deletes the file at the relative path (e.g., "config/KeyBinds.json")
    public static void DeleteData(string relativePath)
    {
        string fullPath = Path.Combine(dataPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log("Deleted: " + fullPath);
        }
        else
        {
            Debug.LogWarning("File to delete not found: " + fullPath);
        }
    }
}
