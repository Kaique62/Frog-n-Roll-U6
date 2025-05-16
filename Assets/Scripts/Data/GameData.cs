using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class GameData
{
    private static string dataPath => Application.persistentDataPath;

    // Salva o dicionário em um caminho relativo (ex: "config/KeyBinds.json")
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

    // Carrega o dicionário de um caminho relativo (ex: "config/KeyBinds.json")
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
            Debug.LogWarning("Data Not Found at: " + fullPath);
            return null;
        }
    }

    // Deleta o arquivo no caminho relativo (ex: "config/KeyBinds.json")
    public static void DeleteData(string relativePath)
    {
        string fullPath = Path.Combine(dataPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log("Data Removed: " + fullPath);
        }
        else
        {
            Debug.LogWarning("Attempted to delete missing file: " + fullPath);
        }
    }
}
