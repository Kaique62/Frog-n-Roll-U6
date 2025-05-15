using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string dataPath => Path.Combine(Application.persistentDataPath);

    public static void Save(Dictionary<dynamic, dynamic> data, string file)
    {
        string filePath = dataPath + file;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Saved to: " + filePath);
    }

    public static Dictionary<dynamic, dynamic> Load(string file)
    {
        string filePath = dataPath + file;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<Dictionary<dynamic, dynamic>>(json);
        }
        else
        {
            Debug.LogWarning("Data Not Found!");
            return null;
        }
    }

    public static void DeleteData(string file)
    {
        string filePath = dataPath + file;
        if (File.Exists(filePath))
            File.Delete(filePath);
        
        Debug.Log("Data Removed!");
    }
}
